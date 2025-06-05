
Param(
	[String]$soundAsset,
	[String]$soundDoc
)
	
$parsedData = Import-Csv -Path $soundAsset

Write-Host Found $parsedData.Count sound items

$word = New-Object -ComObject word.application
$word.Visible = $false
$doc = $word.documents.add()
$doc.Styles["Normal"].ParagraphFormat.SpaceAfter = 0
$doc.Styles["Normal"].ParagraphFormat.SpaceBefore = 0
$margin = 36 # 1.26 cm
$doc.PageSetup.LeftMargin = $margin
$doc.PageSetup.RightMargin = $margin
$doc.PageSetup.TopMargin = $margin
$doc.PageSetup.BottomMargin = $margin
$selection = $word.Selection

$Selection.Font.Name = "Arial"
$Selection.Font.Size = 24
$Selection.Font.bold = 1
$selection.paragraphFormat.Alignment = 1
$selection.TypeText("Sound Info")
$ddd = $selection.TypeParagraph()

$Selection.Font.bold = 0
$Selection.Font.Size = 11
$selection.paragraphFormat.Alignment = 0

for ($i = 0; $i -lt $parsedData.Count; $i++)
{
	$item = $parsedData[$i]
	$fn =  $item.Filename
	$dur = $item.Duration
	$des = $item.Description
	$ts = $item.Transcript
	$al = $item.ActivationList
				
	$table = $selection.Tables.Add($selection.Range(), 5, 2)
	$c1Width = 100
	$c2Width = 400
	
	$table.Cell(1,1).Width = $c1Width
	$table.Cell(1,2).Width = $c2Width
	$table.Cell(2,1).Width = $c1Width
	$table.Cell(2,2).Width = $c2Width
	$table.Cell(3,1).Width = $c1Width
	$table.Cell(3,2).Width = $c2Width
	$table.Cell(4,1).Width = $c1Width
	$table.Cell(4,2).Width = $c2Width
	$table.Cell(5,1).Width = $c1Width
	$table.Cell(5,2).Width = $c2Width
	
	$table.cell(1,1).range.Font.bold = 1
	$table.cell(2,1).range.Font.bold = 1
	$table.cell(3,1).range.Font.bold = 1
	$table.cell(4,1).range.Font.bold = 1
	$table.cell(5,1).range.Font.bold = 1
		
	$table.Cell(1,1).Range.Text = "Wave File:"
	$table.Cell(1,2).Range.Text = $item.Filename
	$table.Cell(2,1).Range.Text = "Description:"
	$table.Cell(2,2).Range.Text = $item.Description
	$table.Cell(3,1).Range.Text = "Duration:"
	$table.Cell(3,2).Range.Text = $item.Duration
	$table.Cell(4,1).Range.Text = "Transcript:"
	$table.Cell(4,2).Range.Text = $item.Transcript
	$table.Cell(5,1).Range.Text = "Activation List:"
	$table.Cell(5,2).Range.Text = $item.ActivationList
	
	$ddd = $selection.EndKey(6)
	$ddd = $selection.TypeParagraph()
}

$doc.SaveAs($soundDoc)
$doc.Close()
$word.Quit()

Write-Host Finished creation of $soundDoc

# Open the Word document
#Start-Process -FilePath $soundDoc

$folder = Split-Path $soundDoc

ii $folder

# Release the Word COM object
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($word) | Out-Null