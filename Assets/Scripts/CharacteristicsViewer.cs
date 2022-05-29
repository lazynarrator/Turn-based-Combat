using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacteristicsViewer : MonoBehaviour
{
    public GameObject fightScene;

    [Header("Prefabs for glow")]
    public GameObject plGlow;
    public GameObject tgGlow;
    public List<GameObject> allTargetGlow = new List<GameObject>();

    private GameObject playerUnit;
    private GameObject targetUnit;

    private GameObject currentPlGlow;
    private GameObject currentTgGlow;
    private SpriteRenderer currentPlayerRender;
    private SpriteRenderer currentTargetRender;
    private SpriteRenderer fightSceneRender;
    private FightController fight;

    private Color playerColor = new Color(1f, 1f, 1f);
    private Color targetColor = new Color(1f, 0.14f, 0.14f);

    [Header("Player characteristics")]
	public List<GameObject> plParams = new List<GameObject>();
    private List<TextMeshProUGUI> plTextParams = new List<TextMeshProUGUI>();

	[Header("Target characteristics")]
	public List<GameObject> tgParams = new List<GameObject>();
    private List<TextMeshProUGUI> tgTextParams = new List<TextMeshProUGUI>();

    public enum State
    {
        Preview,
        Сhoice,
        Attack,
        Attacked
    };

    public State currentState = new State();

    public Button buttonAttack;
    public Button buttonSkip;

    public void StartView(GameObject startPlayer, GameObject startTarget)
    {
        currentState = State.Preview;

        playerUnit = startPlayer;
        targetUnit = startTarget;

        SetGlow();
        SetTextParams();
        PlayerParams(playerUnit, plTextParams);
        PlayerParams(targetUnit, tgTextParams);
    }

    public void NextView(GameObject nextPlayer, GameObject nextTarget)
    {
        currentState = State.Attacked;

        fightSceneRender.enabled = false;

        playerUnit = nextPlayer;
        if (targetUnit == null)
        {
            targetUnit = nextTarget;
            currentTgGlow = Instantiate(tgGlow, targetUnit.transform);
            currentTargetRender = currentTgGlow.GetComponent<SpriteRenderer>();
        }

        currentPlayerRender.enabled = true;
        currentPlGlow.transform.SetParent(nextPlayer.transform, false);
        currentPlayerRender.color = playerColor;
        PlayerParams(playerUnit, plTextParams);

        currentTargetRender.enabled = true;
        currentTgGlow.transform.SetParent(targetUnit.transform, false);
        currentTargetRender.color = targetColor;
        PlayerParams(targetUnit, tgTextParams);
    }

    public void OffGlow()
    {
        currentTargetRender.enabled = false;
        for (int i = 0; i < allTargetGlow.Count; i++)
        {
            allTargetGlow[i].GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    public void OffGlowSkip()
    {
        currentTargetRender.enabled = false;
        currentPlayerRender.enabled = false;
    }

    private void SetGlow()
    {
        currentPlGlow = Instantiate(plGlow, playerUnit.transform);
        currentTgGlow = Instantiate(tgGlow, targetUnit.transform);

        currentPlayerRender = currentPlGlow.GetComponent<SpriteRenderer>();
        currentTargetRender = currentTgGlow.GetComponent<SpriteRenderer>();
        fightSceneRender = fightScene.GetComponent<SpriteRenderer>();
        fight = GetComponent<FightController>();
    }

    private void SetTextParams()
    {
        for (int i = 0; i < plParams.Count; i++)
        {
            TextMeshProUGUI textMeshPro = StartText(plParams[i]);
            plTextParams.Add(textMeshPro);
            textMeshPro = StartText(tgParams[i]);
            tgTextParams.Add(textMeshPro);
        }
    }

	private TextMeshProUGUI StartText(GameObject check)
    {
        TextMeshProUGUI textMeshPro = check.GetComponent<TextMeshProUGUI>();
        return textMeshPro;
    }

    private void PlayerParams(GameObject checkUnit, List<TextMeshProUGUI> checkList)
    {
        Unit unit = checkUnit.GetComponent<Unit>();
        checkList[0].text = unit.Damage[0].ToString() + "-" + unit.Damage[1].ToString();
        checkList[1].text = unit.Accuracy.ToString() + "%";
        checkList[2].text = unit.Evasion.ToString() + "%";
        checkList[3].text = unit.Luck.ToString() + "%";
        checkList[4].text = unit.Health.ToString();
        checkList[5].text = unit.Armor.ToString();
        checkList[6].text = unit.Block.ToString();
        if (unit.Elite == true)
        {
            checkList[7].text = "Да";
        }
        else
        {
            checkList[7].text = "Нет";
        }
    }

    private void FixedUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layer = 1 << 6;
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, float.MaxValue, layer);

        if (currentState == State.Preview)
        {
            if (fightSceneRender.enabled == true)
            {
                fightSceneRender.enabled = false;
            }
            if (buttonAttack.interactable == false || buttonSkip.interactable == false)
            {
                buttonAttack.interactable = true;
                buttonSkip.interactable = true;
            }
        }
        else
        {
            if (buttonAttack.interactable == true || buttonSkip.interactable == true)
            {
                buttonAttack.interactable = false;
                buttonSkip.interactable = false;
            }
        }

        if (hit)
        {
            if (currentState == State.Preview)
            {
                if (currentTargetRender.enabled == false || currentPlayerRender.enabled == false)
                {
                    currentTargetRender.enabled = true;
                    currentPlayerRender.enabled = true;
                }

                if (targetUnit != hit.transform.gameObject)
                {
                    targetUnit = hit.transform.gameObject;
                    currentTgGlow.transform.SetParent(hit.transform, false);

                    if (hit.transform.position.x > 0)
                    {
                        currentTargetRender.color = targetColor;
                    }
                    else
                    {
                        currentTargetRender.color = playerColor;
                    }

                    PlayerParams(targetUnit, tgTextParams);   
                } 
            }
            else if (currentState == State.Сhoice)
            {
                if (hit.transform.position.x > 0)
                {
                    if (currentTargetRender.enabled == false || currentPlayerRender.enabled == false)
                    {
                        currentTargetRender.enabled = true;
                        currentPlayerRender.enabled = true;
                    }

                    if (targetUnit != hit.transform.gameObject)
                    {
                        targetUnit = hit.transform.gameObject;
                        currentTgGlow.transform.SetParent(hit.transform, false);
                        currentTargetRender.color = targetColor;
                        PlayerParams(targetUnit, tgTextParams);
                    }

                    if (Input.GetMouseButton(0))
                    {
                        currentTargetRender.enabled = false;
                        currentPlayerRender.enabled = false;
                        for (int i = 0; i < allTargetGlow.Count; i++)
                        {
                            allTargetGlow[i].GetComponent<SpriteRenderer>().enabled = false;
                        }

                        currentState = State.Attack;
                    }
                }   
            }
            else if(currentState == State.Attack)
            {
                if (fightSceneRender.enabled == false)
                {
                    fightSceneRender.enabled = true;
                    fight.FightProcess(playerUnit, targetUnit);
                }
            }
            else if(currentState == State.Attacked)
            {
                //Do nothing
            }
        }
    }
}
