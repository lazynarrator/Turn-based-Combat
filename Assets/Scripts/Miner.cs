using Spine.Unity;
using UnityEngine;

public class Miner : Unit
{
    public override int[] Damage { get => damage; protected set => Damage = damage; }
    public override int Accuracy { get => accuracy; protected set => throw new System.NotImplementedException(); }
    public override int Evasion { get => evasion; protected set => throw new System.NotImplementedException(); }
    public override int Luck { get => luck; protected set => throw new System.NotImplementedException(); }
    public override int Health { get => health; protected set => Health = health; }
    public override int Armor { get => armor; protected set => Armor = armor; }
    public override int Block { get => block; protected set => Block = block; }
    public override bool Elite { get => elite; protected set => throw new System.NotImplementedException(); }
    public override AnimationReferenceAsset Idle => idle;
    public override AnimationReferenceAsset NormalDamage => normalDamage;
    public override AnimationReferenceAsset CritDamage => critDamage;
    public override AnimationReferenceAsset Damaged => damaged;
    public override bool Move { get => move; protected set => Move = move; }

    public AnimationReferenceAsset idle;
    public AnimationReferenceAsset normalDamage;
    public AnimationReferenceAsset critDamage;
    public AnimationReferenceAsset damaged;

    private int[] damage;
    private int accuracy;
    private int evasion;
    private int luck;
    private int health;
    private int armor;
    private int block;
    private bool elite;

    //для плавного передвижения
    private bool move = false;
    private Vector3 transformPositionOld = new Vector3();
    private Vector3 transformPositionNew = new Vector3();
    private float speed = 1f;
    private float startTime;
    private float journeyLength;

    private enum attackResult
    {
        Miss,
        Normal,
        Crit
    };

    private enum damageResult
    {
        Blocked,
        Dodged,
        ArmorLess,
        ArmorOff,
        DamageTaken
    };

    private void Awake()
    {
        damage = new int[2] { 22, 24 };
        accuracy = StartPercentageValue(85, 101);
        evasion = StartPercentageValue(0, 16);
        luck = StartPercentageValue(0, 41);
        health = 80;
        armor = 0;
        block = 0;
        elite = IsElite();

        if (elite == true)
        {
            damage = new int[2] { 32, 34 };
            GetComponent<SkeletonAnimation>().Skeleton.SetSkin("elite");
        }
    }

    private int StartPercentageValue(int first, int second)
    {
        int value = Random.Range(first, second);
        return value;
    }

    private bool IsElite()
    {
        int value = Random.Range(0, 100);
        if (value > 16)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public override int[] Attack()
    {
        int checkAccuracy = Random.Range(0, 101);  
        //попадание
        if (checkAccuracy <= Accuracy)
        {
            //значение для обычного удара из диапазона
            int normalDamage = Random.Range(Damage[0], Damage[1] + 1);
            int checkLuck = Random.Range(0, 101);
            if (checkLuck <= Luck)
            {
                float critDamage = normalDamage * 1.5f;
                //Debug.Log("Критический удар! " + critDamage);
                int[] result = { (int)critDamage, (int)attackResult.Crit };
                return result;
            }
            else
            {
                //Debug.Log("Удар! " + normalDamage);
                int[] result = { normalDamage, (int)attackResult.Normal };
                return result;
            }
        }
        else
        {
            //Debug.Log("Промах");
            int[] result = { 0, (int)attackResult.Miss };
            return result;
        }
    }

    public override int[] GetDamage(int value)
    {
        if (value > 0)
        {
            if (Block > 0)
            {
                //Debug.Log("Удар заблокирован");
                block--;
                int[] result = { 0, (int)damageResult.Blocked };
                return result;
            }
            else
            {
                int checkEvasion = Random.Range(0, 101);
                if (checkEvasion <= Evasion)
                {
                    //Debug.Log("Уклонение");
                    int[] result = { 0, (int)damageResult.Dodged };
                    return result;
                }
                else
                {
                    if (Armor > 0)
                    {
                        int checkArmor = armor - value;
                        if (checkArmor >= 0)
                        {
                            armor = armor - value;
                            //Debug.Log("Броня уменьшена");
                            int[] result = { 0, (int)damageResult.ArmorLess };
                            return result;
                        }
                        else
                        {
                            armor = 0;
                            health = health - checkArmor;
                            //Debug.Log("Получен урон! " + checkArmor);
                            int[] result = { checkArmor, (int)damageResult.ArmorOff };
                            return result;
                        }
                    }
                    else
                    {
                        health = health - value;
                        //Debug.Log("Получен урон! " + value);
                        int[] result = { value, (int)damageResult.DamageTaken };
                        return result;
                    }
                }
            }
        }
        else
        {
            int[] result = { 0, (int)damageResult.Dodged };
            return result;
        }
    }

    public override void Dead()
    {
        Destroy(gameObject);
    }
    
    public override void Moving(Vector3 first, Vector3 second, float newSpeed)
    {
        move = true;
        transformPositionOld = first;
        transformPositionNew = second;
        speed = newSpeed;
        startTime = Time.time;
        journeyLength = Vector3.Distance(transformPositionOld, transformPositionNew);
    }

    private void ChangeCoord()
    {
        if (move == true)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(transformPositionOld, transformPositionNew, fractionOfJourney);
            if (transform.position == transformPositionNew)
            {
                move = false;
            }
        }
    }

    private void FixedUpdate()
    {
        ChangeCoord();
    }

}
