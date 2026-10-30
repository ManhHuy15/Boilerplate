<#
.SYNOPSIS
    Script đổi tên dự án Boilerplate và khởi tạo Git mới cho Windows (PowerShell).
.EXAMPLE
    .\rename-project.ps1 -NewName MyStore
#>

[CmdletBinding()]
param (
    [Parameter(Position = 0)]
    [string]$NewName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not (Test-Path "BE") -or -not (Test-Path "FE")) {
    Write-Error "Vui lòng chạy script từ thư mục gốc của dự án (nơi chứa thư mục BE và FE)."
    exit 1
}

# 1. Xác định tên dự án hiện tại
$CurrentProjectName = "Boilerplate"
$existingSlnx = Get-ChildItem -Path "BE" -Filter "*.slnx" | Select-Object -First 1
if ($existingSlnx) {
    $CurrentProjectName = $existingSlnx.BaseName
}

$CurrentLower = $CurrentProjectName.ToLowerInvariant()
$CurrentKebab = ([regex]::Replace($CurrentProjectName, '([a-z0-9])([A-Z])', '$1-$2')).ToLowerInvariant()

# 2. Đọc tên mới
if ([string]::IsNullOrWhiteSpace($NewName)) {
    $NewName = Read-Host "Nhập tên dự án mới (viết hoa chữ cái đầu PascalCase, vd: MyStore)"
}

if ($NewName -notmatch '^[A-Za-z][A-Za-z0-9_]*$') {
    Write-Error "Tên dự án '$NewName' không hợp lệ. Phải bắt đầu bằng chữ cái và chỉ chứa chữ/số."
    exit 1
}

$PascalName = $NewName
$LowerName = $NewName.ToLowerInvariant()
$KebabName = ([regex]::Replace($PascalName, '([a-z0-9])([A-Z])', '$1-$2')).ToLowerInvariant()

$CurrentDir = (Get-Item -Path ".").FullName
$CurrentDirName = (Get-Item -Path ".").Name
$ParentDir = (Get-Item -Path "..").FullName
$TargetDir = Join-Path $ParentDir $PascalName

if ($CurrentDirName -ne $PascalName -and (Test-Path $TargetDir)) {
    Write-Error "Thư mục '$TargetDir' đã tồn tại sẵn trong '$ParentDir'. Vui lòng xóa hoặc đổi tên thư mục cũ trước."
    exit 1
}

Write-Host ""
Write-Host "🚀 Đổi tên dự án từ '$CurrentProjectName' sang '$PascalName':" -ForegroundColor Cyan
Write-Host "   - PascalCase : $PascalName"
Write-Host "   - Lowercase  : $LowerName"
Write-Host "   - Kebab-case : $KebabName"
Write-Host ""

# 3. Dọn dẹp bin và obj
Write-Host "🧹 Đang dọn dẹp thư mục build tạm thời (bin, obj)..." -ForegroundColor Yellow
Get-ChildItem -Path "BE" -Recurse -Directory -Include "bin", "obj" -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

# 4. Thay thế nội dung file
Write-Host "📝 Đang cập nhật nội dung các file..." -ForegroundColor Yellow
$extensions = @("*.cs", "*.csproj", "*.slnx", "*.http", "*.json", "*.ts", "*.tsx", "*.md", "*.yml", "*.yaml", "*.example", "Dockerfile")

$files = Get-ChildItem -Path . -Recurse -File | Where-Object {
    $path = $_.FullName
    $path -notmatch '[\\/]\.git[\\/]' -and
    $path -notmatch '[\\/]node_modules[\\/]' -and
    $path -notmatch '[\\/]bin[\\/]' -and
    $path -notmatch '[\\/]obj[\\/]' -and
    $_.Name -ne "rename-project.sh" -and
    $_.Name -ne "rename-project.ps1"
} | Where-Object {
    $item = $_
    $extensions | Where-Object { $item.Name -like $_ }
}

foreach ($file in $files) {
    try {
        $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
        if ($content -match "$CurrentProjectName|$CurrentLower") {
            $updated = $content.Replace("$CurrentKebab-fe", "$KebabName-fe")
            $updated = $updated.Replace("$CurrentKebab-", "$KebabName-")
            $updated = $updated.Replace($CurrentProjectName, $PascalName)
            $updated = $updated.Replace($CurrentLower, $LowerName)
            [System.IO.File]::WriteAllText($file.FullName, $updated, [System.Text.Encoding]::UTF8)
        }
    } catch {
        Write-Warning "Không thể cập nhật file: $($file.FullName)"
    }
}

# 5. Đổi tên file csproj, http, slnx
Write-Host "📁 Đang đổi tên các file project..." -ForegroundColor Yellow
Get-ChildItem -Path "BE" -Recurse -File -Include "*$CurrentProjectName*.csproj", "*$CurrentProjectName*.http" | ForEach-Object {
    $newName = $_.Name.Replace($CurrentProjectName, $PascalName)
    Rename-Item -Path $_.FullName -NewName $newName
}

if (Test-Path "BE\$CurrentProjectName.slnx") {
    Rename-Item -Path "BE\$CurrentProjectName.slnx" -NewName "$PascalName.slnx"
}

# 6. Đổi tên các thư mục Backend
Write-Host "📂 Đang đổi tên các thư mục Backend..." -ForegroundColor Yellow
$layers = @("API", "Application", "Domain", "Infrastructure")
foreach ($layer in $layers) {
    $oldDir = "BE\$CurrentProjectName.$layer"
    if (Test-Path $oldDir) {
        Rename-Item -Path $oldDir -NewName "$PascalName.$layer"
    }
}

Write-Host "✅ Đã đổi tên toàn bộ file và code sang '$PascalName' thành công!" -ForegroundColor Green

# 7. BẮT BUỘC: Xóa Git cũ và khởi tạo Git mới sạch (Bảo vệ Repo gốc)
Write-Host ""
Write-Host "🔒 Đang xóa toàn bộ lịch sử và liên kết Git cũ để bảo vệ repository gốc..." -ForegroundColor Yellow
if (Test-Path ".git") {
    Remove-Item -Path ".git" -Recurse -Force
}
git init -b main
git add .
git commit -m "feat: initialize $PascalName project from boilerplate"
Write-Host "✨ Đã khởi tạo Git mới sạch 100% trên branch 'main' với commit khởi đầu!" -ForegroundColor Green
Write-Host ""
Write-Host "👉 Để liên kết với repository mới của bạn và push code:" -ForegroundColor Cyan
Write-Host "   git remote add origin https://github.com/<your-username>/<your-repo>.git"
Write-Host "   git push -u origin main"

# 8. Đổi tên thư mục gốc ngoài cùng
if ($CurrentDirName -ne $PascalName) {
    Write-Host ""
    Write-Host "📁 Đang đổi tên thư mục gốc ngoài cùng: $CurrentDirName -> $PascalName..." -ForegroundColor Yellow
    try {
        Rename-Item -Path $CurrentDir -NewName $PascalName
        Write-Host "✅ Đã đổi tên thư mục gốc ngoài cùng thành: $PascalName" -ForegroundColor Green
    } catch {
        Write-Warning "Không thể tự động đổi tên thư mục gốc vì VS Code hoặc terminal đang mở thư mục này. Bạn có thể đổi tên folder '$CurrentDirName' thành '$PascalName' từ File Explorer sau khi đóng workspace."
    }
}

Write-Host ""
Write-Host "🎉 HOÀN TẤT! Bạn có thể kiểm tra lại dự án bằng cách chạy:" -ForegroundColor Green
Write-Host "   dotnet build BE\$PascalName.slnx --configuration Release"
Write-Host "   docker compose up --build -d"
