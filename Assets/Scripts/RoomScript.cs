using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class RoomScript : MonoBehaviour
{
    public List<Sprite> Backgrounds;

    private SpriteRenderer spriteRenderer;

	void Start ()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
    public void ChangeBackground(int i)
    {
        if(Backgrounds.Count > i)
            spriteRenderer.sprite = Backgrounds[i];
    }
}