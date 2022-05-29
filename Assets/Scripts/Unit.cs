using Spine.Unity;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    //сила атаки
    public abstract int[] Damage { get; protected set; }
    //точность (шанс попадания) в процентах
    public abstract int Accuracy { get; protected set; }
    //шанс уклонения
    public abstract int Evasion { get; protected set; }
    //шанс критического удара
    public abstract int Luck { get; protected set; }
    //количество здоровья
    public abstract int Health { get; protected set; }
    //количество брони
    public abstract int Armor { get; protected set; }
    //защита от атаки
    public abstract int Block { get; protected set; }
    //элитный юнит или нет
    public abstract bool Elite { get; protected set; }

    //для того, чтобы обращаться к анимации одинакового типа для разных наследников
    public abstract AnimationReferenceAsset Idle { get; }
    public abstract AnimationReferenceAsset NormalDamage { get; }
    public abstract AnimationReferenceAsset CritDamage { get; }
    public abstract AnimationReferenceAsset Damaged { get; }

    public abstract bool Move { get; protected set; }

    public abstract int[] Attack();
    public abstract int[] GetDamage(int value);
    public abstract void Dead();
    public abstract void Moving(Vector3 first, Vector3 second, float newSpeed);

}
