using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class CameraFollow : NetworkBehaviour
{
    public NetworkVariable<FixedString128Bytes> CharacterCode = new(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<Vector3> cameraPosition = new(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<Quaternion> cameraRotation = new(
        Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> poseReady = new(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Camera cam;
    private SpriteRenderer portrait;

    public override void OnNetworkSpawn()
    {
        cam = Camera.main;
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = false;
        foreach (Collider collider in GetComponentsInChildren<Collider>())
            collider.enabled = false;

        var visual = new GameObject("Character Sprite");
        visual.transform.SetParent(transform, false);
        portrait = visual.AddComponent<SpriteRenderer>();
        CharacterCode.OnValueChanged += ApplyCharacterCode;
        if (IsOwner)
            CharacterCode.Value = new FixedString128Bytes(chargen.LoadSavedCharacterCode());
        ApplyCharacterCode(default, CharacterCode.Value);
    }

    public override void OnNetworkDespawn()
    {
        CharacterCode.OnValueChanged -= ApplyCharacterCode;
        if (portrait != null) Destroy(portrait.gameObject);
    }

    private void ApplyCharacterCode(FixedString128Bytes previous, FixedString128Bytes current)
    {
        string code = current.ToString();
        if (!chargen.IsValidCharacterCode(code)) code = chargen.DefaultCode;
        Sprite[] sprites = GameAssets.i.characterSprites;
        int index = int.Parse(code.Substring(24, 2));
        portrait.sprite = sprites[Mathf.Clamp(index, 0, sprites.Length - 1)];
        if (portrait.sprite != null)
            portrait.transform.localScale = Vector3.one * (5f / portrait.sprite.bounds.size.y);
        portrait.enabled = false;
    }

    private void LateUpdate()
    {
        if (!IsSpawned) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        if (IsOwner)
        {
            cameraPosition.Value = cam.transform.position;
            cameraRotation.Value = cam.transform.rotation;
            poseReady.Value = true;
        }
        transform.SetPositionAndRotation(cameraPosition.Value, cameraRotation.Value);
        portrait.enabled = !IsOwner && poseReady.Value;
        portrait.transform.rotation = cam.transform.rotation;
    }
}
