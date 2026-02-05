using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class DragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private MonsterHealth unit;
    private Vector3 offset;

    void Awake()
    {
        unit = GetComponent<MonsterHealth>();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (BattleManager.Instance.startPvP || BattleManager.Instance.winPanel.activeSelf || BattleManager.Instance.losePanel.activeSelf || Char.Instance.activePointerId != -999) return;
        Char.Instance.activePointerId = eventData.pointerId;
        offset = transform.position - GetMouseWorldPos();
        GridManager.Instance.Remove(unit.gridX, unit.gridY);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != Char.Instance.activePointerId || BattleManager.Instance.startPvP || BattleManager.Instance.winPanel.activeSelf || BattleManager.Instance.losePanel.activeSelf) return;
        transform.position = GetMouseWorldPos() + offset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != Char.Instance.activePointerId || BattleManager.Instance.startPvP || BattleManager.Instance.winPanel.activeSelf || BattleManager.Instance.losePanel.activeSelf) return;
        Char.Instance.activePointerId = -999;
        AudioManager.Instance.Play(GameSound.snapSound);
        TrySnap();
    }
    void TrySnap()  //thay đổi vị trí unit
    {
        GridManager grid = GridManager.Instance;

        int x = Mathf.RoundToInt((transform.position.x - grid.origin.x) / grid.cellSize);   //tính tọa độ (x,y) của vị trí mới
        int y = Mathf.RoundToInt((transform.position.y - grid.origin.y+0.08f) / grid.cellSize);

        if (!grid.IsValid(x, y) || y > 2 || (TutorialController.Instance.currentState == TutorialController.TutorialState.Phase1_dragUnit && (x != 2 || y != 0))
            || (TutorialController.Instance.currentState == TutorialController.TutorialState.Phase2_DragMerge && !TutorialController.Instance.isSucessPos(new Vector2(unit.gridX, unit.gridY), new Vector2(x, y))))  //nếu vị trí nằm ngoài grid thì trả về chỗ cũ
        {
            SnapBack();
            if(TutorialController.Instance.currentState == TutorialController.TutorialState.Phase2_DragMerge || TutorialController.Instance.currentState == TutorialController.TutorialState.Phase1_dragUnit) unit.GetComponent<SortingGroup>().sortingOrder = 95;
            return;
        }

        MonsterHealth other = grid.GetUnit(x, y);

        if (other != null)
        {
            if (other.stats.level == unit.stats.level && other.stats.type == unit.stats.type)
            {
                TryMerge(other);
            }
            else
            {
                SwapWith(other);
            }
            return;
        }

        grid.Place(unit, x, y);     //nếu vị trí mới đang không có unit thì đặt unit vào vị trí mới
        if (TutorialController.Instance.currentState == TutorialController.TutorialState.Phase1_dragUnit) TutorialController.Instance.OnDragCompleted();
    }

    void SnapBack()     //trả unit về vị trí cũ trong trường hợp không di chuyển được
    {
        GridManager.Instance.Place(unit, unit.gridX, unit.gridY);
    }

    void TryMerge(MonsterHealth other)   //hàm merge unit
    {
        if (other.stats.level != unit.stats.level)
        {
            SnapBack();
            return;
        }

        // MERGE
        GridManager.Instance.Remove(other.gridX, other.gridY);
        GridManager.Instance.Remove(unit.gridX, unit.gridY);
        MergeTracker.OnMerge();
        GameLog.Log("unit_merge", new
        {
            mergeCount = MergeTracker.mergeCount,
            timeToFirstMerge = MergeTracker.timeToFirstMerge
        });

        GameObject unitObj = Instantiate(Char.Instance.GetUnitPrefabs(unit.stats.level+1, unit.stats.type == MonsterType.Melee));
        MonsterHealth mh = unitObj.GetComponent<MonsterHealth>();
        BattleManager.Instance.playerTeam.Add(unitObj);
        Char.Instance.dataMyTeam.Add(mh);
        mh.SetStats(mh.stats.level);
        AudioManager.Instance.PlayUnitSound(mh.stats.level, unit.stats.type);
        GridManager.Instance.Place(mh, other.gridX, other.gridY);

        unitObj.GetComponent<SortingGroup>().sortingOrder = -other.gridY;
        BattleManager.Instance.playerTeam.Remove(other.gameObject);
        BattleManager.Instance.playerTeam.Remove(unit.gameObject);
        if (TutorialController.Instance != null)
        {
            TutorialController.Instance.OnMergeCompleted();
        }
        Vector3 pos = unitObj.transform.position;
        pos.y += 0.3f;
        PanelManager.Instance.statsUnit.level = mh.stats.level;
        TryUnlockUnit(mh);

        Destroy(other.gameObject);
        Destroy(unit.gameObject);
        VFXManager.Instance.Play(VFXType.Merge, pos);
        Char.Instance.dataMyTeam.RemoveAll(m => m == null);
    }
    void TryUnlockUnit(MonsterHealth mh)
    {
        int index = mh.stats.level - 1;
        if (index < 0) return;
        bool isMelee = mh.stats.type == MonsterType.Melee;
        List<bool> unlockArray = isMelee ? Char.Instance.unlockUnitMelee : Char.Instance.unlockUnitRange;
        if (index >= unlockArray.Count || unlockArray[index]) return;
        unlockArray[index] = true;
        PanelManager.Instance.statsUnit.isMelee = isMelee;
        PanelManager.Instance.OpenPanel(PanelManager.Instance.unlockUnit);
    }

    void SwapWith(MonsterHealth other)
    {
        GridManager grid = GridManager.Instance;

        // Lưu vị trí cũ của unit đang kéo
        int oldX = unit.gridX;
        int oldY = unit.gridY;

        // Vị trí của unit còn lại
        int otherX = other.gridX;
        int otherY = other.gridY;

        // Gỡ unit còn lại khỏi grid
        grid.Remove(otherX, otherY);

        // Đặt unit đang kéo vào vị trí mới
        grid.Place(unit, otherX, otherY);

        // Đặt unit còn lại về vị trí cũ
        grid.Place(other, oldX, oldY);
    }

    Vector3 GetMouseWorldPos() //lấy vị trí chuột
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //chuyển từ tọa độ screen(màn hình điện thoại) sang tọa độ world(Unity)
        pos.z = 0;
        return pos;
    }
}
