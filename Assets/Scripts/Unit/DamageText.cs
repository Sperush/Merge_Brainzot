using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public float moveUpSpeed = 1f;
    public float lifeTime = 1f;

    private TextMeshPro text;
    private Color color;


    void Awake()
    {
        text = GetComponent<TextMeshPro>();
        color = text.color;
    }

    public void SetText(string value) //Hiển thị gía trị dame gây ra trong 1 giây 
    {
        if (text == null) return;
        text.SetText(value);
    }

    public void DamageSize(float damage)
    {
        transform.localScale *= (1f + (damage / 500));
    }

    //private void Start()
    //{
    //    transform.localPosition = new Vector3(Random.Range(-0.7f, 0.7f), 0, 0);

    //}

    void Update()
    {
        if (!BattleManager.Instance.startPvP)
        {
            Destroy(gameObject);
            return;
        }
        // Bay lên
        transform.Translate(Vector3.up * moveUpSpeed * Time.deltaTime);

        // Mờ dần
        color.a -= Time.deltaTime / lifeTime;
        text.color = color;

        // Tự huỷ
        if (color.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}
