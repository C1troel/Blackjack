using UnityEngine;

public class GridPanelPlacer : MonoBehaviour
{
    public GameObject panelPrefab; // Префаб панели
    public int panelCount = 5;     // Количество панелей в дорожке
    public float gap = 0.5f;       // Расстояние между панелями

    private void Start()
    {
        PlaceRoad();
    }

    void PlaceRoad()
    {
        Vector3 startPosition = transform.position;

        // Вычисление размера панели по оси X, чтобы учесть масштаб и расстояние
        float panelWidth = panelPrefab.transform.localScale.x;
        float step = panelWidth + gap;

        for (int i = 0; i < panelCount; i++)
        {
            // Позиция для новой панели
            Vector3 position = startPosition + new Vector3(i * step, 0, 0);

            // Создание панели и установка позиции и поворота
            GameObject panel = Instantiate(panelPrefab, position, Quaternion.Euler(-40, 0, 0));

            // Делаем панель дочерним объектом текущего объекта (дорожка)
            panel.transform.SetParent(transform);
        }
    }
}