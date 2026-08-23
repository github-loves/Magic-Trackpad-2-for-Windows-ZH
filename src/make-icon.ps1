Add-Type -AssemblyName System.Drawing
$zh = 'C:\Users\Allen\AppData\Local\Temp\opencode\mt2fw\src\zh'
$src = 'C:\Users\Allen\Downloads\5d3a4b01-761a-4439-9280-49aa0c14f1bc-removebg-preview.png'

$img = [System.Drawing.Image]::FromFile($src)
$bmp = New-Object System.Drawing.Bitmap(256, 256)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.DrawImage($img, 0, 0, 256, 256)
$g.Dispose()
$img.Dispose()

$ms = New-Object System.IO.MemoryStream
$bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
$pngBytes = $ms.ToArray()
$bmp.Dispose()

$ico = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter($ico)
$bw.Write([UInt16]0)  # reserved
$bw.Write([UInt16]1)  # type icon
$bw.Write([UInt16]1)  # count
$bw.Write([Byte]0)    # width 256
$bw.Write([Byte]0)    # height 256
$bw.Write([Byte]0)    # colors
$bw.Write([Byte]0)    # reserved
$bw.Write([UInt16]1)  # planes
$bw.Write([UInt16]32) # bpp
$bw.Write([UInt32]$pngBytes.Length)
$bw.Write([UInt32]22) # offset
$bw.Write($pngBytes)
$bw.Flush()
[System.IO.File]::WriteAllBytes("$zh\app.ico", $ico.ToArray())
$bw.Dispose()

"ICO written: " + (Get-Item "$zh\app.ico").Length + " bytes"
