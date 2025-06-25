using UnityEngine;

public class GridPanelPlacer : MonoBehaviour
{
    public GameObject panelPrefab; // ������ ������
    public int panelCount = 5;     // ���������� ������� � �������
    public float gap = 0.5f;       // ���������� ����� ��������

    private void Start()
    {
        PlaceRoad();
    }

    void PlaceRoad()
    {
        Vector3 startPosition = transform.position;

        // ���������� ������� ������ �� ��� X, ����� ������ ������� � ����������
        float panelWidth = panelPrefab.transform.localScale.x;
        float step = panelWidth + gap;

        for (int i = 0; i < panelCount; i++)
        {
            // ������� ��� ����� ������
            Vector3 position = startPosition + new Vector3(i * step, 0, 0);

            // �������� ������ � ��������� ������� � ��������
            GameObject panel = Instantiate(panelPrefab, position, Quaternion.Euler(-40, 0, 0));

            // ������ ������ �������� �������� �������� ������� (�������)
            panel.transform.SetParent(transform);
        }
    }
}