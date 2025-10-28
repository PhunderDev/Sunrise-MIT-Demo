using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField]
    private int MaxHP, HP;

    [SerializeField]
    private UnityEvent OnDeath;


    private void Awake()
    {
    }

    private void OnValidate()
    {
        MaxHP = Mathf.Clamp(MaxHP, 0, Mathf.Abs(MaxHP));
        SetHP(HP);
    }






    public void SetHP(int Amount)
    {
        HP = Mathf.Clamp(Amount, 0, MaxHP);
    }

    public void SetMaxHP(int Amount)
    {
        MaxHP = Amount;
    }

    public void ApplyDamage(int Amount)
    {
        SetHP(HP - Amount);
        if (HP <= 0) OnDeath.Invoke();
    }

    public int GetHP()
    {
        return HP;
    }
}
