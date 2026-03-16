using UnityEngine;

[ExecuteInEditMode] // Cho phép chạy ngay trong Editor khi bạn kéo thanh trượt
public class GrassIndividualProperty : MonoBehaviour
{
    [Header("Cài đặt Gió & Hiệu ứng (Ghi đè Material)")]

    [Range(0f, 1f)]
    public float WindIntensity = 0.28f; // Mặc định theo ảnh

    public float WindScale = 0.5f;      // Mặc định theo ảnh

    public float WindSpeed = 1.08f;     // Mặc định theo ảnh

    [Range(0f, 1f)]
    public float YMaskInfluence = 1.0f; // Mặc định theo ảnh

    public bool MaskFromBottom = true;  // Checkbox

    public float ExternalInfluence = 1.0f; // Lực tác động mặc định

    // Cache ID của Shader để chạy nhanh hơn (đỡ phải tìm tên string mỗi frame)
    private static readonly int _windIntensityID = Shader.PropertyToID("_WindIntensity");
    private static readonly int _windScaleID = Shader.PropertyToID("_WindScale");
    private static readonly int _windSpeedID = Shader.PropertyToID("_WindSpeed");
    private static readonly int _yMaskInfluenceID = Shader.PropertyToID("_YMaskInfluence");
    private static readonly int _maskFromBottomID = Shader.PropertyToID("_MaskFromBottom");
    private static readonly int _externalInfluenceID = Shader.PropertyToID("_ExternalInfluence");

    private SpriteRenderer _renderer;
    private MaterialPropertyBlock _propBlock;

    private void OnEnable()
    {
        UpdatePropertyBlock();
    }

    private void OnValidate()
    {
        // Hàm này chạy mỗi khi bạn thay đổi giá trị trong Inspector
        UpdatePropertyBlock();
    }

    void UpdatePropertyBlock()
    {
        if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        // 1. Lấy trạng thái hiện tại
        _renderer.GetPropertyBlock(_propBlock);

        // 2. Gán các giá trị từ Inspector vào Property Block
        _propBlock.SetFloat(_windIntensityID, WindIntensity);
        _propBlock.SetFloat(_windScaleID, WindScale);
        _propBlock.SetFloat(_windSpeedID, WindSpeed);
        _propBlock.SetFloat(_yMaskInfluenceID, YMaskInfluence);

        // Lưu ý: Boolean trong Shader Graph thực chất là Float (0 hoặc 1)
        _propBlock.SetFloat(_maskFromBottomID, MaskFromBottom ? 1f : 0f);

        _propBlock.SetFloat(_externalInfluenceID, ExternalInfluence);

        // 3. Áp dụng vào Renderer
        _renderer.SetPropertyBlock(_propBlock);
    }
}