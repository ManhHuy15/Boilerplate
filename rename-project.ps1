<#
.SYNOPSIS
    Script đổi tên dự án Boilerplate và khởi tạo Git mới cho Windows (PowerShell).
.EXAMPLE
    .\rename-project.ps1 -NewName MyStore -ResetGit
#>

[CmdletBinding()]
param (
    [Parameter(Position = 0)]
    [string]$NewName,

    [switch]$ResetGit
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not (Test-Path "BE\Boilerplate.slnx") -and -not (Test-Path "BE")) {
    Write-Error "Vui lòng chạy script từ thư mục gốc của dự án (nơi chứa thư mục BE và FE)."
    exit 1
}

if ([string]::IsNullOrWhiteSpace($NewName)) {
    $NewName = Read-Host "Nhập tên dự án mới (viết hoa chữ cái đầu PascalCase, vd: MyStore)"
}

if ($NewName -notmatch '^[A-Za-z][A-Za-z0-9_]*$') {
    Write-Error "Tên dự án '$NewName' không hợp lệ. Phải bắt đầu bằng chữ cái và chỉ chứa ký tự chữ/số."
    exit 1
}

$PascalName = $NewName
$LowerName = $NewName.ToLowerInvariant()
$KebabName = ([regex]::Replace($PascalName, '([a-z0-9])([A-Z])', '$1-$2')).ToLowerInvariant()

Write-Host ""
Write-Host "🚀 Chuẩn bị đổi tên Boilerplate sang: $PascalName" -ForegroundColor Cyan
Write-Host "   - PascalCase : $PascalName"
Write-Host "   - Lowercase  : $LowerName"
Write-Host "   - Kebab-case : $KebabName"
Write-Host ""

# 1. Dọn dẹp bin và obj
Write-Host "🧹 Đang dọn dẹp thư mục build tạm thời (bin, obj)..." -ForegroundColor Yellow
Get-ChildItem -Path "BE" -Recurse -Directory -Include "bin", "obj" -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

# 2. Thay thế nội dung file
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
        if ($content -match "Boilerplate|boilerplate") {
            $updated = $content.Replace("boilerplate-fe", "$KebabName-fe")
            $updated = $updated.Replace("boilerplate-", "$KebabName-")
            $updated = $updated.Replace("Boilerplate", $PascalName)
            $updated = $updated.Replace("boilerplate", $LowerName)
            [System.IO.File]::WriteAllText($file.FullName, $updated, [System.Text.Encoding]::UTF8)
        }
    } catch {
        Write-Warning "Không thể cập nhật file: $($file.FullName)"
    }
}

# 3. Đổi tên file csproj, http, slnx
Write-Host "📁 Đang đổi tên các file project..." -ForegroundColor Yellow
Get-ChildItem -Path "BE" -Recurse -File -Include "*Boilerplate*.csproj", "*Boilerplate*.http" | ForEach-Object {
    $newName = $_.Name.Replace("Boilerplate", $PascalName)
    Rename-Item -Path $_.FullName -NewName $newName
}

if (Test-Path "BE\Boilerplate.slnx") {
    Rename-Item -Path "BE\Boilerplate.slnx" -NewName "$PascalName.slnx"
}

# 4. Đổi tên các thư mục Backend
Write-Host "📂 Đang đổi tên các thư mục Backend..." -ForegroundColor Yellow
$layers = @("API", "Application", "Domain", "Infrastructure")
foreach ($layer in $layers) {
    $oldDir = "BE\Boilerplate.$layer"
    if (Test-Path $oldDir) {
        Rename-Item -Path $oldDir -NewName "$PascalName.$layer"
    }
}

Write-Host "✅ Đã đổi tên toàn bộ dự án sang '$PascalName' thành công!" -ForegroundColor Green

# 5. Xử lý Git
if (-not $ResetGit) {
    Write-Host ""
    $confirm = Read-Host "Bạn có muốn xóa lịch sử Git cũ và khởi tạo Git mới cho repo mới không? (y/N)"
    if ($confirm -eq "y" -or $confirm -eq "Y") {
        $ResetGit = $true
    }
}

if ($ResetGit) {
    Write-Host "🔄 Đang reset Git repository..." -ForegroundColor Yellow
    if (Test-Path ".git") {
        Remove-Item -Path ".git" -Recurse -Force
    }
    git init -b main
    git add .
    git commit -m "feat: initialize $PascalName project from boilerplate"
    Write-Host "✨ Đã khởi tạo Git mới trên branch 'main' với commit khởi đầu!" -ForegroundColor Green
    Write-Host ""
    Write-Host "👉 Tiếp theo, bạn có thể liên kết repo mới và push code:" -ForegroundColor Cyan
    Write-Host "   git remote add origin https://github.com/<your-username>/<your-repo>.git"
    Write-Host "   git push -u origin main"
} else {
    Write-Host "ℹ️  Bỏ qua bước reset Git. Bạn có thể tự quản lý Git theo ý muốn."
}

# 6. Đổi tên thư mục gốc ngoài cùng
$CurrentDir = (Get-Item -Path ".").FullName
$CurrentDirName = (Get-Item -Path ".").Name
$ParentDir = (Get-Item -Path "..").FullName
$TargetDir = Join-Path $ParentDir $PascalName

if ($CurrentDirName -ne $PascalName) {
    Write-Host ""
    Write-Host "📁 Đang đổi tên thư mục gốc ngoài cùng: $CurrentDirName -> $PascalName..." -ForegroundColor Yellow
    if (Test-Path $TargetDir) {
        Write-Warning "Thư mục '$TargetDir' đã tồn tại sẵn, bỏ qua bước đổi tên thư mục gốc."
    } else {
        try {
            Rename-Item -Path $CurrentDir -NewName $PascalName
            Set-Location -Path $TargetDir -ErrorAction SilentlyContinue
            Write-Host "✅ Đã đổi tên thư mục gốc ngoài cùng thành: $PascalName" -ForegroundColor Green
        } catch {
            Write-Warning "Không thể tự động đổi tên thư mục gốc vì VS Code hoặc terminal đang mở thư mục này. Bạn có thể đổi tên folder '$CurrentDirName' thành '$PascalName' từ File Explorer sau khi đóng workspace."
        }
    }
}

Write-Host ""
Write-Host "🎉 HOÀN TẤT! Bạn có thể kiểm tra lại dự án bằng cách chạy:" -ForegroundColor Green
Write-Host "   dotnet build BE\$PascalName.slnx --configuration Release"
Write-Host "   docker compose up --build -d"
