#!/usr/bin/env bash
set -euo pipefail

# ==============================================================================
# Script đổi tên dự án Boilerplate và khởi tạo Git mới
# Sử dụng: ./rename-project.sh [TenDuAnMoi]
# Ví dụ:   ./rename-project.sh MyStore
# ==============================================================================

# 1. Kiểm tra vị trí chạy script
if [ ! -d "BE" ] || [ ! -d "FE" ]; then
    echo "❌ Lỗi: Vui lòng chạy script này từ thư mục gốc của dự án (nơi chứa thư mục BE và FE)."
    exit 1
fi

# Chặn ngay nếu đang chạy trong Trash (Thùng rác)
if [[ "$(pwd)" =~ Trash|\.local/share/Trash ]]; then
    echo "❌ Lỗi: Bạn đang chạy script bên trong thư mục Thùng rác (Trash)!"
    echo "   Vui lòng di chuyển dự án ra một thư mục làm việc chính thức (ví dụ: ~/Work/ hoặc Desktop) trước khi đổi tên."
    exit 1
fi

# 2. Xác định tên dự án hiện tại (mặc định là Boilerplate hoặc tên đã đổi trước đó)
CURRENT_PROJECT_NAME="Boilerplate"
if [ ! -f "BE/Boilerplate.slnx" ]; then
    EXISTING_SLNX=$(find BE -maxdepth 1 -name "*.slnx" 2>/dev/null | head -n 1)
    if [ -n "$EXISTING_SLNX" ]; then
        CURRENT_PROJECT_NAME=$(basename "$EXISTING_SLNX" .slnx)
    fi
fi

CURRENT_LOWER="$(echo "$CURRENT_PROJECT_NAME" | tr '[:upper:]' '[:lower:]')"
CURRENT_KEBAB="$(echo "$CURRENT_PROJECT_NAME" | sed -r 's/([a-z0-9])([A-Z])/\1-\2/g' | tr '[:upper:]' '[:lower:]')"

# 3. Đọc tham số tên mới
NEW_NAME="${1:-}"

if [ -z "$NEW_NAME" ]; then
    read -rp "👉 Nhập tên dự án mới (viết hoa chữ cái đầu PascalCase, vd: MyStore): " NEW_NAME
fi

if [[ ! "$NEW_NAME" =~ ^[A-Za-z][A-Za-z0-9_]*$ ]]; then
    echo "❌ Lỗi: Tên dự án '$NEW_NAME' không hợp lệ. Phải bắt đầu bằng chữ cái và chỉ chứa chữ/số (vd: MyStore, AcmeApi)."
    exit 1
fi

PASCAL_NAME="$NEW_NAME"
LOWER_NAME="$(echo "$PASCAL_NAME" | tr '[:upper:]' '[:lower:]')"
KEBAB_NAME="$(echo "$PASCAL_NAME" | sed -r 's/([a-z0-9])([A-Z])/\1-\2/g' | tr '[:upper:]' '[:lower:]')"

CURRENT_DIR="$(pwd)"
CURRENT_DIR_NAME="$(basename "$CURRENT_DIR")"
PARENT_DIR="$(dirname "$CURRENT_DIR")"
TARGET_DIR="$PARENT_DIR/$PASCAL_NAME"

# Kiểm tra thư mục đích có bị trùng lặp ở thư mục cha không
if [ "$CURRENT_DIR_NAME" != "$PASCAL_NAME" ] && [ -d "$TARGET_DIR" ]; then
    echo "❌ Lỗi: Thư mục '$TARGET_DIR' đã tồn tại sẵn trong '$PARENT_DIR'!"
    echo "   Vui lòng xóa hoặc đổi tên thư mục '$TARGET_DIR' cũ trước, hoặc chọn một tên dự án mới khác."
    exit 1
fi

echo ""
echo "🚀 Đổi tên dự án từ '$CURRENT_PROJECT_NAME' sang '$PASCAL_NAME':"
echo "   - PascalCase : $PASCAL_NAME"
echo "   - Lowercase  : $LOWER_NAME"
echo "   - Kebab-case : $KEBAB_NAME"
echo ""

# 4. Dọn dẹp bin và obj cũ
echo "🧹 Đang dọn dẹp thư mục build tạm thời (bin, obj)..."
find BE -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} + 2>/dev/null || true

# 5. Thay thế nội dung trong các file nguồn
echo "📝 Đang cập nhật nội dung các file..."
FILES_TO_REPLACE=$(find . \
    -type f \
    ! -path "*/.git/*" \
    ! -path "*/node_modules/*" \
    ! -path "*/bin/*" \
    ! -path "*/obj/*" \
    ! -name "rename-project.sh" \
    ! -name "rename-project.ps1" \
    \( -name "*.cs" \
       -o -name "*.csproj" \
       -o -name "*.slnx" \
       -o -name "*.http" \
       -o -name "*.json" \
       -o -name "*.ts" \
       -o -name "*.tsx" \
       -o -name "*.md" \
       -o -name "*.yml" \
       -o -name "*.yaml" \
       -o -name "*.example" \
       -o -name "Dockerfile" \)
)

for file in $FILES_TO_REPLACE; do
    if [ -f "$file" ]; then
        sed -i \
            -e "s/${CURRENT_KEBAB}-fe/${KEBAB_NAME}-fe/g" \
            -e "s/${CURRENT_KEBAB}-/${KEBAB_NAME}-/g" \
            -e "s/${CURRENT_PROJECT_NAME}/${PASCAL_NAME}/g" \
            -e "s/${CURRENT_LOWER}/${LOWER_NAME}/g" \
            "$file"
    fi
done

# 6. Đổi tên các file csproj và http
echo "📁 Đang đổi tên các file project..."
find BE \( -name "*${CURRENT_PROJECT_NAME}*.csproj" -o -name "*${CURRENT_PROJECT_NAME}*.http" \) | while read -r filepath; do
    dir=$(dirname "$filepath")
    filename=$(basename "$filepath")
    new_filename="${filename//$CURRENT_PROJECT_NAME/$PASCAL_NAME}"
    mv "$filepath" "$dir/$new_filename"
done

# Đổi tên solution file
if [ -f "BE/${CURRENT_PROJECT_NAME}.slnx" ]; then
    mv "BE/${CURRENT_PROJECT_NAME}.slnx" "BE/${PASCAL_NAME}.slnx"
fi

# 7. Đổi tên các thư mục Backend
echo "📂 Đang đổi tên các thư mục Backend..."
for layer in API Application Domain Infrastructure; do
    if [ -d "BE/${CURRENT_PROJECT_NAME}.${layer}" ]; then
        mv "BE/${CURRENT_PROJECT_NAME}.${layer}" "BE/${PASCAL_NAME}.${layer}"
    fi
done

echo "✅ Đã đổi tên toàn bộ file và code sang '$PASCAL_NAME' thành công!"

# 8. BẮT BUỘC: Xóa Git cũ và khởi tạo Git mới sạch (Bảo vệ Repo gốc, tránh push nhầm)
echo ""
echo "🔒 Đang xóa toàn bộ lịch sử và liên kết Git cũ để bảo vệ repository gốc..."
rm -rf .git
git init -b main
git add .
git commit -m "feat: initialize $PASCAL_NAME project from boilerplate"
echo "✨ Đã khởi tạo Git mới sạch 100% trên branch 'main' với commit khởi đầu!"
echo ""
echo "👉 Để liên kết với repository mới của bạn và push code:"
echo "   git remote add origin https://github.com/<your-username>/<your-repo>.git"
echo "   git push -u origin main"

# 9. Đổi tên thư mục gốc ngoài cùng
if [ "$CURRENT_DIR_NAME" != "$PASCAL_NAME" ]; then
    echo ""
    echo "📁 Đang đổi tên thư mục gốc ngoài cùng: $CURRENT_DIR_NAME -> $PASCAL_NAME..."
    mv "$CURRENT_DIR" "$TARGET_DIR"
    echo "✅ Đã đổi tên thư mục gốc ngoài cùng thành: $TARGET_DIR"
fi

echo ""
echo "🎉 HOÀN TẤT! Bạn có thể kiểm tra lại dự án bằng cách chạy:"
echo "   dotnet build BE/${PASCAL_NAME}.slnx --configuration Release"
echo "   docker compose up --build -d"
