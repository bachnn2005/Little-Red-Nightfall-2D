using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteExtractor
{
    [MenuItem("Assets/Export Sprites to Files", false, 0)]
    static void ExportSprites()
    {
        // Lấy object đang được chọn
        Object selectedObject = Selection.activeObject;

        if (selectedObject == null || !(selectedObject is Texture2D))
        {
            Debug.LogError("Hãy chọn một file ảnh (Texture) trong Project trước!");
            return;
        }

        Texture2D sourceTex = selectedObject as Texture2D;
        string path = AssetDatabase.GetAssetPath(sourceTex);
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

        // Bắt buộc phải bật Read/Write để đọc pixel
        if (!ti.isReadable)
        {
            ti.isReadable = true;
            ti.SaveAndReimport();
        }

        // Lấy danh sách các sprite đã cắt (Slicing) trong file đó
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

        // Tạo folder mới cùng tên với ảnh để chứa các file xuất ra
        string dirPath = Path.GetDirectoryName(path) + "/" + sourceTex.name + "_Exported";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        int count = 0;
        foreach (Object asset in assets)
        {
            // Chỉ xử lý nếu nó là Sprite (bỏ qua file Texture gốc)
            if (asset is Sprite sprite)
            {
                // Tạo Texture mới từ vùng cắt của Sprite
                Texture2D output = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] pixels = sourceTex.GetPixels(
                    (int)sprite.rect.x,
                    (int)sprite.rect.y,
                    (int)sprite.rect.width,
                    (int)sprite.rect.height
                );

                output.SetPixels(pixels);
                output.Apply();

                // Lưu ra file PNG
                byte[] bytes = output.EncodeToPNG();
                string filename = dirPath + "/" + sprite.name + ".png";
                File.WriteAllBytes(filename, bytes);
                count++;
            }
        }

        Debug.Log($"Đã xuất thành công {count} file ảnh vào thư mục: {dirPath}");
        AssetDatabase.Refresh(); // Refresh lại Unity để hiện file
    }

    // Chỉ hiện nút này khi click vào Texture
    [MenuItem("Assets/Export Sprites to Files", true)]
    static bool ValidateExportSprites()
    {
        return Selection.activeObject is Texture2D;
    }
}