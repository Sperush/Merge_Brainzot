using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; // Cần thiết
using System.Collections;
using UnityEngine.Rendering;

public class TutorialController : MonoBehaviour
{
    public static TutorialController Instance;

    [Header("UI References")]
    public GameObject tutorialCanvas;
    public GameObject darkMaskSprite;
    public TextMeshProUGUI instructionText;
    public RectTransform handPointer;

    [Header("Buttons Objects")]
    public GameObject btnBuyMeleeObj;
    public GameObject btnBuyRangeObj;
    public GameObject btnBattleObj;
    public GameObject goldDisplayUI;

    private int originalOrder;

    // --- THÊM BIẾN ĐỂ QUẢN LÝ ---
    private Tween currentHandTween; // Lưu trữ Tween hiện tại để Kill chính xác
    private Coroutine mergeCoroutine; // Lưu trữ Coroutine để dừng nếu Merge quá nhanh

    public enum TutorialState
    {
        None,
        Phase1_BuyMelee, Phase1_CheckGold, Phase1_BuyRange, Phase1_dragUnit, Phase1_ClickBattle,
        Phase2_BuyMelee, Phase2_DragMerge, Phase2_ClickBattle
    }
    public TutorialState currentState = TutorialState.None;

    private Transform originalParent;
    private int originalSiblingIndex;
    public Transform unit1;
    public Transform unit2;
    private Vector2 TargetPos;
    private Vector2 StartPos;

    void Awake() { Instance = this; }

    void OnEnable() { Button2D.OnButton2DClicked += HandleButtonClicked; }
    void OnDisable()
    {
        Button2D.OnButton2DClicked -= HandleButtonClicked;
        KillCurrentTween(); // Dọn dẹp khi disable
    }

    // --- EVENT LISTENER ---
    void HandleButtonClicked(ButtonType type)
    {
        // ... (Logic Phase 1 giữ nguyên) ...
        if (currentState == TutorialState.Phase1_BuyMelee && type == ButtonType.SpawnMelee)
        {
            SetState(TutorialState.Phase1_CheckGold);
        }
        else if (currentState == TutorialState.Phase1_BuyRange && type == ButtonType.SpawnRange)
        {
            if (mergeCoroutine != null) StopCoroutine(mergeCoroutine);
            mergeCoroutine = StartCoroutine(ShowDragUnit());
        }
        else if (currentState == TutorialState.Phase1_ClickBattle && type == ButtonType.Battle)
        {
            EndPhase1();
        }
        else if (currentState == TutorialState.Phase2_BuyMelee && type == ButtonType.SpawnMelee)
        {
            // LƯU COROUTINE LẠI ĐỂ QUẢN LÝ
            if (mergeCoroutine != null) StopCoroutine(mergeCoroutine);
            mergeCoroutine = StartCoroutine(SetupMergeDragState());
        }
        else if (currentState == TutorialState.Phase2_ClickBattle && type == ButtonType.Battle)
        {
            EndPhase2();
        }
    }

    // ... (Các hàm StartPhase1, StartPhase2_Merge, SetState GIỮ NGUYÊN) ...
    public void StartPhase1()
    {
        tutorialCanvas.SetActive(true);
        darkMaskSprite.SetActive(true);
        SetState(TutorialState.Phase1_BuyMelee);
    }

    public void StartPhase2_Merge()
    {
        tutorialCanvas.SetActive(true);
        darkMaskSprite.SetActive(true);
        SetState(TutorialState.Phase2_BuyMelee);
    }
    public bool isSucessPos(Vector2 start, Vector2 target)
    {
        return StartPos == start && TargetPos == target || StartPos == target && TargetPos == start;
    }

    public void SetState(TutorialState state)
    {
        currentState = state;
        switch (state)
        {
            case TutorialState.Phase1_BuyMelee:
                HighlightUI(btnBuyMeleeObj, Noti.Get("buy_melee"));
                ShowHandAt(btnBuyMeleeObj.transform.position);
                if (btnBuyMeleeObj.GetComponent<SortingGroup>())
                    btnBuyMeleeObj.GetComponent<SortingGroup>().sortingOrder = 101;
                break;
            case TutorialState.Phase1_CheckGold:
                if (btnBuyMeleeObj.GetComponent<SortingGroup>())
                    btnBuyMeleeObj.GetComponent<SortingGroup>().sortingOrder = -10;
                RestoreUI(btnBuyMeleeObj);
                HighlightUI(goldDisplayUI, Noti.Get("need_gold_buy"));
                handPointer.gameObject.SetActive(false);
                Invoke(nameof(MoveToBuyRange), 2.5f);
                break;
            case TutorialState.Phase1_BuyRange:
                RestoreUI(goldDisplayUI);
                HighlightUI(btnBuyRangeObj, Noti.Get("buy_range"));
                ShowHandAt(btnBuyRangeObj.transform.position);
                break;
            case TutorialState.Phase1_ClickBattle:
                HighlightUI(btnBattleObj, Noti.Get("click_start"));
                ShowHandAt(btnBattleObj.transform.position);
                break;

            case TutorialState.Phase2_BuyMelee:
                UnitSpawner.Instance.txtCostMelee.GetComponent<MeshRenderer>().sortingOrder = 95;
                HighlightUI(btnBuyMeleeObj, Noti.Get("buy_to_merge"));
                ShowHandAt(btnBuyMeleeObj.transform.position);
                break;
            case TutorialState.Phase2_ClickBattle:
                RestoreUI(btnBuyRangeObj);
                HighlightUI(btnBattleObj, Noti.Get("click_start"));
                ShowHandAt(btnBattleObj.transform.position);
                break;
        }
    }
    // ...

    // --- SỬA LẠI LOGIC SETUP MERGE ---
    IEnumerator SetupMergeDragState()
    {
        currentState = TutorialState.Phase2_DragMerge;
        UnitSpawner.Instance.txtCostMelee.GetComponent<MeshRenderer>().sortingOrder = -10;
        RestoreUI(btnBuyMeleeObj);
        darkMaskSprite.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        // KIỂM TRA LẠI: Nếu trong lúc chờ 0.2s mà người chơi đã Merge xong rồi thì dừng luôn
        if (currentState != TutorialState.Phase2_DragMerge) yield break;

        var grid = GridManager.Instance;
        Transform startUnit = null;
        Transform endUnit = null;
        int count = 0;

        for (int x = 0; x < grid.columns; x++)
        {
            for (int y = 0; y < grid.rows; y++)
            {
                var u = grid.GetUnit(x, y);
                if (u != null && u.stats.type == MonsterType.Melee && u.gameObject.activeSelf && u.CompareTag("Player"))
                {
                    if (count == 0) startUnit = u.transform;
                    if (count == 1) endUnit = u.transform;
                    count++;
                }
            }
        }

        if (startUnit != null && endUnit != null)
        {
            unit1 = startUnit;
            unit2 = endUnit;
            unit1.GetComponent<SortingGroup>().sortingOrder = 95;
            unit2.GetComponent<SortingGroup>().sortingOrder = 95;

            handPointer.gameObject.SetActive(true);

            // --- TÍNH TOÁN VỊ TRÍ ---
            Canvas canvas = tutorialCanvas.GetComponent<Canvas>();
            RectTransform canvasRect = tutorialCanvas.GetComponent<RectTransform>();

            Vector2 GetUIPos(Vector3 worldPos)
            {
                Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) return screenPos;
                else
                {
                    Vector2 localPoint;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.worldCamera, out localPoint);
                    return canvasRect.TransformPoint(localPoint);
                }
            }

            Vector3 uiStartPos = GetUIPos(startUnit.position);
            Vector3 uiEndPos = GetUIPos(endUnit.position);
            uiStartPos.z = 0; uiEndPos.z = 0;

            StartPos = grid.ConvertToPosGrid(uiStartPos.x, uiStartPos.y);
            TargetPos = grid.ConvertToPosGrid(uiEndPos.x, uiEndPos.y);
            // --- SỬA LỖI DOTWEEN Ở ĐÂY ---
            KillCurrentTween(); // Kill cái cũ nếu có

            Sequence seq = DOTween.Sequence();
            seq.Append(handPointer.DOMove(uiStartPos, 0f));
            seq.Append(handPointer.DOScale(0.8f, 0.2f));
            seq.Append(handPointer.DOMove(uiEndPos, 1.5f));
            seq.Append(handPointer.DOScale(1f, 0.2f));
            seq.SetLoops(-1);

            // Gán vào biến quản lý và Link với GameObject (để an toàn)
            currentHandTween = seq;
            currentHandTween.SetLink(handPointer.gameObject);
        }
    }
    IEnumerator ShowDragUnit()
    {
        currentState = TutorialState.Phase1_dragUnit;

        RestoreUI(btnBuyRangeObj);
        darkMaskSprite.SetActive(true);
        instructionText.text = Noti.Get("drag_here");

        yield return new WaitForSeconds(0.2f);

        // KIỂM TRA LẠI: Nếu trong lúc chờ 0.2s mà người chơi đã Merge xong rồi thì dừng luôn
        if (currentState != TutorialState.Phase1_dragUnit) yield break;
        Vector3 posA = GridManager.Instance.GetWorldPos(4,0);
        Vector3 posB = GridManager.Instance.GetWorldPos(2,0);
        unit1 = GridManager.Instance.GetUnit(4, 0).transform;
        unit1.GetComponent<SortingGroup>().sortingOrder = 95;
        // Reset vị trí về A
        handPointer.position = posA;
        // --- SỬA LỖI DOTWEEN Ở ĐÂY ---
        KillCurrentTween(); // Kill cái cũ nếu có
        // Sử dụng DOTween để tạo Sequence
        Sequence seq = DOTween.Sequence();

        // Hiệu ứng: Hiện tay -> Di chuyển đến B -> Ẩn -> Lặp lại
        seq.Append(handPointer.GetComponent<CanvasGroup>().DOFade(1, 0.2f)); // Hiện lên
        seq.Append(handPointer.DOMove(posB, 1.0f).SetEase(Ease.InOutQuad)); // Di chuyển
        seq.Append(handPointer.GetComponent<CanvasGroup>().DOFade(0, 0.2f)); // Mờ đi
        seq.AppendInterval(0.5f); // Nghỉ một chút
        seq.SetLoops(-1);

        currentHandTween = seq;
        currentHandTween.SetLink(handPointer.gameObject);
    }

    // --- SỬA LẠI HÀM ON MERGE COMPLETED ---
    public void OnMergeCompleted()
    {
        Debug.Log("Tutorial: OnMergeCompleted called!");

        if (currentState == TutorialState.Phase2_DragMerge)
        {
            // 1. Dừng Coroutine đang chờ (nếu có)
            if (mergeCoroutine != null) StopCoroutine(mergeCoroutine);

            // 2. Kill Tween một cách an toàn (False = Stop, không Complete)
            KillCurrentTween();

            // 4. Reset trạng thái
            if (unit1 != null) unit1.GetComponent<SortingGroup>().sortingOrder = -unit1.GetComponent<MonsterHealth>().gridY;
            if (unit2 != null) unit2.GetComponent<SortingGroup>().sortingOrder = -unit2.GetComponent<MonsterHealth>().gridY;
            tutorialCanvas.SetActive(false);
        }
    }
    // --- SỬA LẠI HÀM ON DRAG COMPLETED ---
    public void OnDragCompleted()
    {
        Debug.Log("Tutorial: OnDragCompleted called!");

        if (currentState == TutorialState.Phase1_dragUnit)
        {
            // 1. Dừng Coroutine đang chờ (nếu có)
            if (mergeCoroutine != null) StopCoroutine(mergeCoroutine);

            // 2. Kill Tween một cách an toàn (False = Stop, không Complete)
            KillCurrentTween();

            // 4. Reset trạng thái
            if (unit1 != null) unit1.GetComponent<SortingGroup>().sortingOrder = -unit1.GetComponent<MonsterHealth>().gridY;

            SetState(TutorialState.Phase1_ClickBattle);
        }
    }
    // Hàm tiện ích để Kill Tween
    void KillCurrentTween()
    {
        if (currentHandTween != null && currentHandTween.IsActive())
        {
            currentHandTween.Kill();
            currentHandTween = null;
        }
        // Kill luôn trên object cho chắc chắn
        handPointer.DOKill();
    }

    void MoveToBuyRange() { SetState(TutorialState.Phase1_BuyRange); }

    void EndPhase1()
    {
        RestoreUI(btnBattleObj);
        tutorialCanvas.SetActive(false);
    }
    void EndPhase2()
    {
        if (mergeCoroutine != null) StopCoroutine(mergeCoroutine);
        KillCurrentTween();
        RestoreUI(btnBattleObj);
        tutorialCanvas.SetActive(false);
        currentState = TutorialState.None;
        tutorialCanvas.GetComponent<Canvas>().sortingOrder = 3;
    }

    void HighlightUI(GameObject target, string text)
    {
        if (target == null) return;
        instructionText.text = text;
        darkMaskSprite.SetActive(true);

        SortingGroup maskSR = darkMaskSprite.GetComponent<SortingGroup>();
        if (maskSR != null) maskSR.sortingOrder = 10;

        SortingGroup targetSR = target.GetComponent<SortingGroup>();
        if (targetSR != null)
        {
            originalOrder = targetSR.sortingOrder;
            targetSR.sortingOrder = 95;
        }
    }

    void RestoreUI(GameObject target)
    {
        if (target == null) return;
        SortingGroup targetSR = target.GetComponent<SortingGroup>();
        if (targetSR != null) targetSR.sortingOrder = originalOrder;
        darkMaskSprite.SetActive(false);
    }
    
    // --- HÀM CHUYỂN ĐỔI TỌA ĐỘ (FIX LỖI) ---
    Vector3 GetUIPos(Vector3 worldPos)
    {
        Canvas canvas = tutorialCanvas.GetComponent<Canvas>();
        RectTransform canvasRect = tutorialCanvas.GetComponent<RectTransform>();

        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return screenPos;
        }
        else
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.worldCamera, out localPoint);
            return canvasRect.TransformPoint(localPoint);
        }
    }

    void ShowHandAt(Vector3 worldPos)
    {
        handPointer.gameObject.SetActive(true);

        // 1. Kill tween cũ
        KillCurrentTween();

        // 2. --- QUAN TRỌNG NHẤT: RESET TRẠNG THÁI ---
        // Phải trả kích thước về 1 trước khi chạy tween mới.
        // Nếu không, nếu tween cũ dừng lúc đang nhỏ, tween mới sẽ bị kẹt.
        handPointer.localScale = Vector3.one;

        // 3. Tính toán vị trí UI (đoạn code fix tọa độ cũ)
        Vector3 uiPos = GetUIPos(worldPos);
        uiPos.z = 0;
        handPointer.position = uiPos;

        // 4. Chạy Tween mới
        currentHandTween = handPointer.DOScale(0.8f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        currentHandTween.SetLink(handPointer.gameObject);
    }
}