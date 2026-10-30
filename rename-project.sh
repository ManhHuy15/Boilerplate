#!/usr/bin/env bash
set -euo pipefail

# ==============================================================================
# Script đổi tên dự án Boilerplate và khởi tạo Git mới
# Sử dụng: ./rename-project.sh [TenDuAnMoi] [--reset-git]
# Ví dụ:   ./rename-project.sh MyStore --reset-git
# ==============================================================================

# Kiểm tra thư mục gốc dự án
if [ ! -f "BE/Boilerplate.slnx" ] && [ ! -d "BE" ]; then
    echo "❌ Lỗi: Vui lòng chạy script này từ thư mục gốc của dự án (nơi chứa thư mục BE và FE)."
    exit 1
fi

NEW_NAME="${1:-}"
RESET_GIT=false

# Kiểm tra cờ --reset-git trong tham số
for arg in "$@"; do
    if [ "$arg" = "--reset-git" ]; then
        RESET_GIT=true
    fi
done

# Nhập tên dự án nếu chưa truyền qua command line
if [ -z "$NEW_NAME" ] || [ "$NEW_NAME" = "--reset-git" ]; then
    read -rp "👉 Nhập tên dự án mới (viết hoa chữ cái đầu PascalCase, vd: MyStore): " NEW_NAME
fi

# Validate tên dự án (chỉ chứa chữ cái, số, bắt đầu bằng chữ cái)
if [[ ! "$NEW_NAME" =~ ^[A-Za-z][A-Za-z0-9_]*$ ]]; then
    echo "❌ Lỗi: Tên dự án '$NEW_NAME' không hợp lệ. Phải bắt đầu bằng chữ cái và chỉ chứa ký tự chữ/số (vd: MyStore, AcmeApi)."
    exit 1
fi

# Chuyển đổi tên sang các dạng casing
PASCAL_NAME="$NEW_NAME"
LOWER_NAME="$(echo "$PASCAL_NAME" | tr '[:upper:]' '[:lower:]')"

# Kebab case (MyStore -> my-store)
KEBAB_NAME="$(echo "$PASCAL_NAME" | sed -r 's/([a-z0-9])([A-Z])/\1-\2/g' | tr '[:upper:]' '[:lower:]')"

echo ""
echo "🚀 Chuẩn bị đổi tên Boilerplate sang: $PASCAL_NAME"
echo "   - PascalCase : $PASCAL_NAME"
echo "   - Lowercase  : $LOWER_NAME"
echo "   - Kebab-case : $KEBAB_NAME"
echo ""

# Dọn dẹp bin và obj cũ trong BE để tránh lỗi build
echo "🧹 Đang dọn dẹp thư mục build tạm thời (bin, obj)..."
find BE -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} + 2>/dev/null || true

# Thay thế nội dung trong các file nguồn
echo "📝 Đang cập nhật nội dung các file..."

# Danh sách phần mở rộng cần thay thế
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
        # Thay thế Boilerplate -> $PASCAL_NAME
        # Thay thế boilerplate- -> $KEBAB_NAME-
        # Thay thế boilerplate -> $LOWER_NAME
        sed -i \
            -e "s/boilerplate-fe/${KEBAB_NAME}-fe/g" \
            -e "s/boilerplate-/${KEBAB_NAME}-/g" \
            -e "s/Boilerplate/${PASCAL_NAME}/g" \
            -e "s/boilerplate/${LOWER_NAME}/g" \
            "$file"
    fi
done

# Đổi tên các file .csproj và .http
echo "📁 Đang đổi tên các file project..."
find BE -name "*Boilerplate*.csproj" -o -name "*Boilerplate*.http" | while read -r filepath; do
    dir=$(dirname "$filepath")
    filename=$(basename "$filepath")
    new_filename="${filename//Boilerplate/$PASCAL_NAME}"
    mv "$filepath" "$dir/$new_filename"
done

# Đổi tên solution file
if [ -f "BE/Boilerplate.slnx" ]; then
    mv "BE/Boilerplate.slnx" "BE/${PASCAL_NAME}.slnx"
fi

# Đổi tên các thư mục Backend
echo "📂 Đang đổi tên các thư mục Backend..."
for layer in API Application Domain Infrastructure; do
    if [ -d "BE/Boilerplate.${layer}" ]; then
        mv "BE/Boilerplate.${layer}" "BE/${PASCAL_NAME}.${layer}"
    fi
done

echo "✅ Đã đổi tên toàn bộ dự án sang '$PASCAL_NAME' thành công!"

# Xử lý Git
if [ "$RESET_GIT" = false ]; then
    echo ""
    read -rp "👉 Bạn có muốn xóa lịch sử Git cũ và khởi tạo Git mới cho repo mới không? (y/N): " CONFIRM_GIT
    if [[ "$CONFIRM_GIT" =~ ^[Yy]$ ]]; then
        RESET_GIT=true
    fi
fi

if [ "$RESET_GIT" = true ]; then
    echo "🔄 Đang reset Git repository..."
    rm -rf .git
    git init -b main
    git add .
    git commit -m "feat: initialize $PASCAL_NAME project from boilerplate"
    echo "✨ Đã khởi tạo Git mới trên branch 'main' với commit khởi đầu!"
    echo ""
    echo "👉 Tiếp theo, bạn có thể liên kết repo mới và push code:"
    echo "   git remote add origin https://github.com/<your-username>/<your-repo>.git"
    echo "   git push -u origin main"
else
    echo "ℹ️  Bỏ qua bước reset Git. Bạn có thể tự quản lý Git theo ý muốn."
fi

# Đổi tên thư mục gốc ngoài cùng (nếu tên thư mục hiện tại khác tên mới)
CURRENT_DIR="$(pwd)"
CURRENT_DIR_NAME="$(basename "$CURRENT_DIR")"
PARENT_DIR="$(dirname "$CURRENT_DIR")"
TARGET_DIR="$PARENT_DIR/$PASCAL_NAME"

if [ "$CURRENT_DIR_NAME" != "$PASCAL_NAME" ]; then
    echo ""
    echo "📁 Đang đổi tên thư mục gốc ngoài cùng: $CURRENT_DIR_NAME -> $PASCAL_NAME..."
    if [ -d "$TARGET_DIR" ]; then
        echo "⚠️  Thư mục '$TARGET_DIR' đã tồn tại sẵn, bỏ qua bước đổi tên thư mục gốc."
    else
        mv "$CURRENT_DIR" "$TARGET_DIR"
        cd "$TARGET_DIR" 2>/dev/null || true
        echo "✅ Đã đổi tên thư mục gốc ngoài cùng thành: $PASCAL_NAME"
    fi
fi

echo ""
echo "🎉 HOÀN TẤT! Bạn có thể kiểm tra lại dự án bằng cách chạy:"
echo "   dotnet build BE/${PASCAL_NAME}.slnx --configuration Release"
echo "   docker compose up --build -d"
