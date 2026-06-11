using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPanel : MonoBehaviour
{
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private CircleLayoutGroup bulletLayout;

    private readonly List<Bullet> bullets = new();

    public void Init(WeaponData data)
    {
        while (bullets.Count < data.MagazineSize)
        {
            bullets.Add(Instantiate(bulletPrefab, bulletLayout.transform));
        }

        while (bullets.Count > data.MagazineSize)
        {
            var lastIndex = bullets.Count - 1;
            bullets[lastIndex].gameObject.SetActive(false);
            Destroy(bullets[lastIndex].gameObject);
            bullets.RemoveAt(lastIndex);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)bulletLayout.transform);
        SetAmmo(data.MagazineSize);
    }

    public void SetAmmo(int ammo)
    {
        for (var i = 0; i < bullets.Count; i++)
        {
            if (i < ammo)
            {
                bullets[i].Activate();
                continue;
            }

            bullets[i].Inactivate();
        }
    }
}
