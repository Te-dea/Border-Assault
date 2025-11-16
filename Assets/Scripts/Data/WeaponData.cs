using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "PixelRogue/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName = "Sword";
    public int damage = 10;
    public float attackRange = 0.5f;
    public float cooldown = 0.5f;
    public Sprite icon;
}