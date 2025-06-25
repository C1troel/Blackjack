using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace JSG.FortuneSpinWheel
{

    public class FortuneSpinWheel : MonoBehaviour
    {
        public RewardData[] m_RewardData;
        public Image m_CircleBase;
        public Image[] m_RewardPictures;
        public Text[] m_RewardCounts;
        public GameObject m_RewardPanel;
        public Text m_RewardFinalText;
        public Image m_RewardFinalImage;
        [HideInInspector]
        public bool m_IsSpinning = false;
        [HideInInspector]
        public float m_SpinSpeed = 0;
        [HideInInspector]
        public float m_Rotation = 0;

        public Image m_SpinButton;

        [HideInInspector]
        public int m_RewardNumber = -1;

        
        [SerializeField]private GameObject wheel;
        [SerializeField]private GameObject inputBlock;
        [SerializeField]private float startOffset = 30;

        private int m_PredefinedReward = -1;

        void Start()
        {
            m_Rotation = 0;
            m_IsSpinning = false;
            m_RewardNumber = -1;

            for (int i = 0; i < m_RewardData.Length; i++)
            {

                m_RewardPictures[i].sprite = m_RewardData[i].m_Icon;
                if (m_RewardData[i].m_Count > 0)
                {
                    m_RewardCounts[i].text = "+" + m_RewardData[i].m_Count.ToString();
                }
                else
                {
                    m_RewardCounts[i].gameObject.SetActive(false);
                }
            }
        }

        void Update()
        {
            if (m_IsSpinning)
            {
                m_RewardPanel.SetActive(false);

                // Уменьшение скорости вращения
                if (m_SpinSpeed > 2)
                {
                    m_SpinSpeed -= 4 * Time.deltaTime;
                }
                else
                {
                    m_SpinSpeed -= 0.3f * Time.deltaTime;
                }

                m_Rotation += 100 * Time.deltaTime * m_SpinSpeed;
                m_CircleBase.transform.localRotation = Quaternion.Euler(0, 0, m_Rotation);

                for (int i = 0; i < m_RewardData.Length; i++)
                {
                    m_RewardPictures[i].transform.rotation = Quaternion.identity;
                }

                if (m_SpinSpeed <= 0)
                {
                    m_SpinSpeed = 0;
                    m_IsSpinning = false;

                    if (m_PredefinedReward != -1)
                    {
                        Debug.Log($"m_PredefinedReward is not -1!");
                        int segments = m_RewardData.Length;

                        // Угол одного сегмента
                        float segmentAngle = 360f / segments;

                        // Рассчёт целевого угла с учётом смещения
                        float targetAngle = 360f * 3 + m_PredefinedReward * segmentAngle - startOffset;

                        // Текущий угол
                        float currentAngle = m_Rotation % 360f;

                        // Дополнительный угол, чтобы "докрутить" до нужного сегмента
                        float additionalAngle = targetAngle - currentAngle;

                        // Если угол отрицательный, корректируем его
                        if (additionalAngle < 0)
                        {
                            additionalAngle += 360f;
                        }

                        m_Rotation += additionalAngle;
                        m_CircleBase.transform.localRotation = Quaternion.Euler(0, 0, m_Rotation);
                        m_RewardNumber = m_PredefinedReward; // Устанавливаем номер награды
                    }
                    else
                    {
                        // Стандартное поведение
                        m_RewardNumber = (int)((m_Rotation % 360) / (360 / m_RewardData.Length));
                    }

                    m_PredefinedReward = -1;
                    DisableWheel();
                }

            }
            else if (m_RewardNumber != -1)
            {
                m_RewardPictures[m_RewardNumber].transform.localScale = (1 + 0.2f * Mathf.Sin(10 * Time.time)) * Vector3.one;
            }
        }

        private void DisableWheel()
        {
            wheel.SetActive(false);
            inputBlock.SetActive(false);
            Reset();
        }

        public void SetReward(int rewardIndex)
        {
            if (rewardIndex >= 0 && rewardIndex < m_RewardData.Length)
            {
                m_PredefinedReward = rewardIndex;
            }
            else
            {
                Debug.LogError("Invalid reward index!");
            }
        }

        /*IEnumerator ShowRewardMenu(int seconds)
        {
            RewardData reward = m_RewardData[m_RewardNumber];
            yield return new WaitForSeconds(seconds);
            if (reward.m_Type != "nothing")
            {
                m_RewardPanel.gameObject.SetActive(true);
                m_RewardFinalText.text = reward.m_Count.ToString();
                m_RewardFinalImage.sprite = reward.m_Icon;
                yield return new WaitForSeconds(2);
            }

            yield return new WaitForSeconds(.1f);
            Reset();
        }*/

        public void StartSpin()
        {
            wheel.SetActive(true);
            inputBlock.SetActive(false);

            if (!m_IsSpinning)
            {
                m_SpinSpeed = Random.Range(4f, 14f);
                m_IsSpinning = true;
                m_RewardNumber = -1;
                /*m_SpinButton.gameObject.SetActive(false);*/
            }
        }

        public void Reset()
        {
            m_Rotation = 0;
            m_CircleBase.transform.localRotation = Quaternion.identity;
            m_IsSpinning = false;
            m_RewardNumber = -1;
            /*m_SpinButton.gameObject.SetActive(true);
            m_RewardPanel.SetActive(false);*/
        }
    }
}
