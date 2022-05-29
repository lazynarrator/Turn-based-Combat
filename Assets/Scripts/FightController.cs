using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static CharacteristicsViewer;

public class FightController : MonoBehaviour
{
    [Header("Prefab for unit")]
    public GameObject unit;

    [Header("Spawns")]
    public List<Transform> plSpawns = new List<Transform>();
    public List<Transform> tgSpawns = new List<Transform>();

    public TextMeshProUGUI textPlayer;
    public TextMeshProUGUI textTarget;
    public Canvas exitCanvas;

    private List<GameObject> players = new List<GameObject>();
    private List<GameObject> targets = new List<GameObject>();
    private List<GameObject> tempPlayers = new List<GameObject>();
    private List<GameObject> tempTargets = new List<GameObject>();
    private Vector3 tempHeroPosition = new Vector3();
    private Vector3 tempEnemyPosition = new Vector3();

    private Unit heroUnit;
    private Unit enemyUnit;

    private CharacteristicsViewer viewer;
    private string foregroundLayer = "Foreground";
    private string playerLayer = "Player Ground";
    private string heroText;
    private string enemyText;
    private AnimationReferenceAsset attackAnimation;
    private AnimationReferenceAsset damageAnimation;

    private enum currentUnit
    {
        Player,
        Target
    };

    private void Start()
    {
        for (int i = 0; i < plSpawns.Count; i++)
        {
            GameObject player = Instantiate(unit);
            player.transform.position = plSpawns[i].position;
            players.Add(player);
        }
        GameObject firstUnit = NextUnit(currentUnit.Player);
        heroUnit = firstUnit.GetComponent<Unit>();

        for (int i = 0; i < tgSpawns.Count; i++)
        {
            GameObject target = Instantiate(unit);
            target.transform.position = tgSpawns[i].position;
            target.transform.rotation = Quaternion.Euler(0f, 179f, 0f);
            targets.Add(target);
            if (i == 0)
            {
                enemyUnit = target.GetComponent<Unit>();
            }
        }

        viewer = GetComponent<CharacteristicsViewer>();
        viewer.StartView(firstUnit, enemyUnit.gameObject);
    }

    private GameObject NextUnit(currentUnit unit)
    {
        if (unit == currentUnit.Player)
        {
            if (players.Count > 0)
            {
                if (tempPlayers.Count == 0)
                {
                    tempPlayers = new List<GameObject>(players);
                    tempPlayers = Shuffle(tempPlayers);
                }
                else
                {
                    tempPlayers.RemoveAt(0);

                    if (tempPlayers.Count == 0)
                    {
                        tempPlayers = new List<GameObject>(players);
                        tempPlayers = Shuffle(tempPlayers);
                    }
                }
                return tempPlayers[0];
            }
            else
            {
                return null;
            } 
        }
        else
        {
            if (targets.Count > 0)
            {
                if (tempTargets.Count == 0)
                {
                    tempTargets = new List<GameObject>(targets);
                    tempTargets = Shuffle(tempTargets);
                }
                else
                {
                    tempTargets.RemoveAt(0);

                    if (tempTargets.Count == 0)
                    {
                        tempTargets = new List<GameObject>(targets);
                        tempTargets = Shuffle(tempTargets);
                    }
                }
                return tempTargets[0];
            }
            else
            {
                return null;
            }
        }
    }

    private List<GameObject> Shuffle(List<GameObject> units)
    {
        for (int i = 0; i < units.Count; i++)
        {
            GameObject temp = units[i];
            int randomIndex = Random.Range(i, units.Count);
            units[i] = units[randomIndex];
            units[randomIndex] = temp;
        }
        return units;
    }

    public void FightButton()
    {
        viewer.OffGlow();
        viewer.currentState = State.Сhoice;
    }

    public void FightProcess(GameObject hero, GameObject enemy)
    {
        heroUnit = hero.GetComponent<Unit>();
        enemyUnit = enemy.GetComponent<Unit>();

        Vector3 plVector3 = new Vector3(plSpawns[0].position.x + 0.2f, plSpawns[0].position.y, plSpawns[0].position.z);
        Vector3 tgVector3 = new Vector3(tgSpawns[0].position.x - 0.2f, tgSpawns[0].position.y, tgSpawns[0].position.z);
        tempHeroPosition = hero.transform.position;
        tempEnemyPosition = enemy.transform.position;

        if (heroUnit.transform.position.x < 0)
        {
            float delta1 = Mathf.Abs(tempHeroPosition.x - plVector3.x);
            float delta2 = Mathf.Abs(tempEnemyPosition.x - tgVector3.x);

            heroUnit.Moving(tempHeroPosition, plVector3, 1f * delta1);
            enemyUnit.Moving(tempEnemyPosition, tgVector3, 1f * delta2);

            textPlayer.GetComponent<RectTransform>().anchoredPosition = new Vector2(-130f, 200f);
            textTarget.GetComponent<RectTransform>().anchoredPosition = new Vector2(130f, 200f);
        }
        else
        {
            float delta1 = Mathf.Abs(tempHeroPosition.x - tgVector3.x);
            float delta2 = Mathf.Abs(tempEnemyPosition.x - plVector3.x);

            heroUnit.Moving(tempHeroPosition, tgVector3, 1f * delta1);
            enemyUnit.Moving(tempEnemyPosition, plVector3, 1f * delta2);

            textPlayer.GetComponent<RectTransform>().anchoredPosition = new Vector2(130f, 200f);
            textTarget.GetComponent<RectTransform>().anchoredPosition = new Vector2(-130f, 200f);
        }

        heroUnit.GetComponent<Renderer>().sortingLayerName = foregroundLayer;
        enemyUnit.GetComponent<Renderer>().sortingLayerName = foregroundLayer;

        int[] attackValues = heroUnit.Attack();
        int[] damageValues = enemyUnit.GetDamage(attackValues[0]);

        heroText = AttackMsg(attackValues);
        enemyText = DamageMsg(damageValues);

        if (attackValues[1] == 0 || attackValues[1] == 1)
           attackAnimation = heroUnit.NormalDamage; 
        else
           attackAnimation = heroUnit.CritDamage; 

        if (damageValues[1] == 0 || damageValues[1] == 1 || damageValues[1] == 2)
           damageAnimation = enemyUnit.Idle; 
        else
           damageAnimation = enemyUnit.Damaged; 

        if (Mathf.Abs(tempHeroPosition.x) >= Mathf.Abs(tempEnemyPosition.x))
           StartCoroutine(WaitSlowMove(heroUnit)); 
        else
           StartCoroutine(WaitSlowMove(enemyUnit)); 
    }

    public void SkipButton()
    {
        viewer.OffGlowSkip();
        StartCoroutine(AfterAttack());
    }

    IEnumerator WaitSlowMove(Unit unit)
    {
        yield return new WaitUntil(() => !unit.GetComponent<Unit>().Move);
        AttackAnimation(attackAnimation, false);
    }

    private void AttackAnimation(AnimationReferenceAsset animation, bool loop)
    {
        textPlayer.text = heroText;
        heroUnit.GetComponent<SkeletonAnimation>().state.ClearTracks();
        Spine.TrackEntry trackAttack = heroUnit.GetComponent<SkeletonAnimation>().state.AddAnimation(0, animation, loop, 0);
        trackAttack.Event += DamageAnimation;
        trackAttack.Complete += EndAnimation;
    }

    private void DamageAnimation(Spine.TrackEntry track, Spine.Event e)
    {
        if (e.Data.Name == "Hit")
        {
            textTarget.text = enemyText;

            if (damageAnimation == enemyUnit.Damaged)
            {
                enemyUnit.GetComponent<SkeletonAnimation>().state.ClearTracks();
                enemyUnit.GetComponent<SkeletonAnimation>().state.AddAnimation(0, damageAnimation, false, 0);
            }
        }
    }

    private void EndAnimation(Spine.TrackEntry track)
    {
        heroUnit.GetComponent<SkeletonAnimation>().state.AddAnimation(1, heroUnit.Idle, true, 0);
        enemyUnit.GetComponent<SkeletonAnimation>().state.AddAnimation(1, enemyUnit.Idle, true, 0);

        heroUnit.transform.position = tempHeroPosition;
        enemyUnit.transform.position = tempEnemyPosition;

        if (viewer.currentState == State.Attack)
        {
            if (enemyUnit.Health <= 0)
            {
                targets.Remove(enemyUnit.gameObject);
                tempTargets.Remove(enemyUnit.gameObject);
                enemyUnit.Dead();
                for (int i = 0; i < targets.Count; i++)
                {
                    targets[i].transform.position = tgSpawns[i].position;
                }
            }

            StartCoroutine(AfterAttack());   
        }
        else if (viewer.currentState == State.Attacked)
        {
            heroUnit.GetComponent<Renderer>().sortingLayerName = playerLayer;
            enemyUnit.GetComponent<Renderer>().sortingLayerName = playerLayer;

            if (enemyUnit.Health <= 0)
            {
                players.Remove(enemyUnit.gameObject);
                tempPlayers.Remove(enemyUnit.gameObject);
                enemyUnit.Dead();
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].transform.position = plSpawns[i].position;
                }
            }

            GameObject nextHero = NextUnit(currentUnit.Player);
            if (nextHero)
            {
                GameObject nextTarget = targets[0];
                viewer.NextView(nextHero, nextTarget);
                viewer.currentState = State.Preview;
            }
            else
            {
                exitCanvas.GetComponent<UITransitions>().ExitWindow("Проигрыш");
                //Debug.Log("Проигрыш");
            }
        }
    }

    private IEnumerator AfterAttack()
    {
        yield return new WaitForSeconds(0.2f);

        heroUnit.GetComponent<Renderer>().sortingLayerName = playerLayer;
        if (enemyUnit)
        {
            enemyUnit.GetComponent<Renderer>().sortingLayerName = playerLayer;
        }

        viewer.currentState = State.Attacked;
        viewer.fightScene.GetComponent<SpriteRenderer>().enabled = false;

        StartCoroutine(EnemyAttack());
    }

    private IEnumerator EnemyAttack()
    {
        yield return new WaitForSeconds(0.2f);

        viewer.fightScene.GetComponent<SpriteRenderer>().enabled = true;

        GameObject firstEnemy = NextUnit(currentUnit.Target);
        if (firstEnemy)
        { 
            int attacked = Random.Range(0, players.Count);
            GameObject firstHero = players[attacked];
            FightProcess(firstEnemy, firstHero);
        }
        else
        {
            exitCanvas.GetComponent<UITransitions>().ExitWindow("Выигрыш");
            //Debug.Log("Выигрыш");
        }
    }

    private string AttackMsg(int[] attackValues)
    {
        string msg;
        switch (attackValues[1])
        {
            case 0:
                msg = "Промах";
                break;
            case 1:
                msg = "Удар! " + attackValues[0];
                break;
            case 2:
                msg = "Критический удар! " + attackValues[0];
                break;
            default:
                msg = "";
                break;
        }
        return msg;
    }

    private string DamageMsg(int[] damageValues)
    {
        string msg;
        switch (damageValues[1])
        {
            case 0:
                msg = "Блок";
                break;
            case 1:
                msg = "Уклонение";
                break;
            case 2:
                msg = "Броня уменьшена";
                break;
            case 3:
                msg = "Получен урон! " + damageValues[0];
                break;
            case 4:
                msg = "Получен урон! " + damageValues[0];
                break;
            default:
                msg = "";
                break;
        }
        return msg;
    }

}
