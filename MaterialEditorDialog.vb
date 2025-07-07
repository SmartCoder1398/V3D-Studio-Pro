Imports System.Drawing
Imports System.Windows.Forms
Imports System.IO
Imports System.Xml.Serialization
Imports Newtonsoft.Json
Imports System.ComponentModel
Imports System.Linq

Public Class MaterialEditorDialog
    Inherits Form

    Private materialListBox As ListBox
    Private previewPanel As Panel
    Private propertyGrid As PropertyGrid
    Private currentMaterial As Material
    Private materials As New Dictionary(Of String, Material)
    Private materialLibraryPath As String = Path.Combine(Application.StartupPath, "Materials")
    Private isUpdating As Boolean = False

    ' Add texture preview panels
    Private diffuseTexturePanel As Panel
    Private normalTexturePanel As Panel
    Private specularTexturePanel As Panel

    Public Sub New()
        Try
            Me.Text = "Material Editor"
            Me.Size = New Size(1000, 700)
            Me.StartPosition = FormStartPosition.CenterScreen

            ' Ensure the library path exists
            If Not Directory.Exists(materialLibraryPath) Then
                Directory.CreateDirectory(materialLibraryPath)
            End If

            InitializeComponents()
            LoadDefaultMaterials()
        Catch ex As Exception
            MessageBox.Show($"Error initializing Material Editor: {ex.Message}", "Error",
                           MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.DialogResult = DialogResult.Cancel
            Me.Close()
        End Try
    End Sub

    Private Sub InitializeComponents()
        ' Main split container
        Dim mainSplitter As New SplitContainer With {
            .Dock = DockStyle.Fill,
            .Orientation = Orientation.Vertical,
            .SplitterDistance = 250
        }

        ' Left panel - Material list
        Dim leftPanel As New Panel With {.Dock = DockStyle.Fill}
        Dim lblMaterials As New Label With {
            .Text = "Materials:",
            .Location = New Point(10, 10),
            .AutoSize = True
        }

        materialListBox = New ListBox With {
            .Location = New Point(10, 35),
            .Size = New Size(230, 300),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        }

        ' Buttons
        Dim btnNew As New Button With {.Text = "New", .Location = New Point(10, 345), .Width = 70}
        Dim btnDelete As New Button With {.Text = "Delete", .Location = New Point(85, 345), .Width = 70}
        Dim btnClone As New Button With {.Text = "Clone", .Location = New Point(160, 345), .Width = 70}

        ' Save/Load buttons
        Dim btnSave As New Button With {.Text = "Save", .Location = New Point(10, 375), .Width = 70}
        Dim btnLoad As New Button With {.Text = "Load", .Location = New Point(85, 375), .Width = 70}
        Dim btnExport As New Button With {.Text = "Export", .Location = New Point(160, 375), .Width = 70}

        leftPanel.Controls.AddRange({lblMaterials, materialListBox, btnNew, btnDelete, btnClone,
                                   btnSave, btnLoad, btnExport})

        ' Right panel - Split into preview and properties
        Dim rightSplitter As New SplitContainer With {
            .Dock = DockStyle.Fill,
            .Orientation = Orientation.Horizontal,
            .SplitterDistance = 250
        }

        ' Preview section with tabs
        Dim previewTabControl As New TabControl With {
            .Dock = DockStyle.Fill
        }

        ' 3D Preview tab
        Dim preview3DTab As New TabPage("3D Preview")
        previewPanel = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.Black,
            .BorderStyle = BorderStyle.Fixed3D
        }
        preview3DTab.Controls.Add(previewPanel)

        ' Texture preview tab
        Dim textureTab As New TabPage("Textures")
        Dim textureContainer = CreateTexturePreviewPanel()
        textureTab.Controls.Add(textureContainer)

        previewTabControl.TabPages.Add(preview3DTab)
        previewTabControl.TabPages.Add(textureTab)

        ' Property editor section
        Dim propertyContainer As New Panel With {
            .Dock = DockStyle.Fill
        }

        ' Create custom property tabs
        Dim propertyTabs As New TabControl With {
            .Dock = DockStyle.Fill
        }

        ' Basic properties tab
        Dim basicTab As New TabPage("Basic")
        Dim basicPanel = CreateBasicPropertiesPanel()
        basicTab.Controls.Add(basicPanel)

        ' Maps tab
        Dim mapsTab As New TabPage("Maps")
        Dim mapsPanel = CreateMapsPanel()
        mapsTab.Controls.Add(mapsPanel)

        ' Advanced tab
        Dim advancedTab As New TabPage("Advanced")
        propertyGrid = New PropertyGrid With {
            .Dock = DockStyle.Fill,
            .ToolbarVisible = False
        }
        advancedTab.Controls.Add(propertyGrid)

        propertyTabs.TabPages.Add(basicTab)
        propertyTabs.TabPages.Add(mapsTab)
        propertyTabs.TabPages.Add(advancedTab)

        propertyContainer.Controls.Add(propertyTabs)

        rightSplitter.Panel1.Controls.Add(previewTabControl)
        rightSplitter.Panel2.Controls.Add(propertyContainer)

        mainSplitter.Panel1.Controls.Add(leftPanel)
        mainSplitter.Panel2.Controls.Add(rightSplitter)

        ' Dialog buttons
        Dim buttonPanel As New Panel With {.Height = 40, .Dock = DockStyle.Bottom}
        Dim btnApply As New Button With {.Text = "Apply", .Location = New Point(620, 8), .Width = 75}
        Dim btnOK As New Button With {.Text = "OK", .Location = New Point(700, 8), .DialogResult = DialogResult.OK}
        Dim btnCancel As New Button With {.Text = "Cancel", .Location = New Point(780, 8), .DialogResult = DialogResult.Cancel}

        buttonPanel.Controls.AddRange({btnApply, btnOK, btnCancel})

        Me.Controls.Add(mainSplitter)
        Me.Controls.Add(buttonPanel)

        ' Event handlers
        AddHandler materialListBox.SelectedIndexChanged, AddressOf MaterialSelected
        AddHandler btnNew.Click, AddressOf CreateNewMaterial
        AddHandler btnDelete.Click, AddressOf DeleteMaterial
        AddHandler btnClone.Click, AddressOf CloneMaterial
        AddHandler btnSave.Click, AddressOf SaveMaterial
        AddHandler btnLoad.Click, AddressOf LoadMaterial
        AddHandler btnExport.Click, AddressOf ExportMaterialLibrary
        AddHandler btnApply.Click, AddressOf ApplyChanges
        AddHandler previewPanel.Paint, AddressOf DrawPreview
        AddHandler propertyGrid.PropertyValueChanged, AddressOf PropertyChanged
    End Sub

    Private Function CreateBasicPropertiesPanel() As Panel
        Dim panel As New Panel With {.Dock = DockStyle.Fill, .AutoScroll = True}
        Dim y As Integer = 20

        ' Diffuse Color
        Dim lblDiffuse As New Label With {
            .Text = "Diffuse Color:",
            .Location = New Point(20, y),
            .Width = 100
        }
        Dim diffuseColorPanel As New Panel With {
            .Location = New Point(130, y - 2),
            .Size = New Size(50, 20),
            .BorderStyle = BorderStyle.FixedSingle,
            .Cursor = Cursors.Hand
        }
        Dim btnDiffuse As New Button With {
            .Text = "...",
            .Location = New Point(185, y - 2),
            .Size = New Size(30, 20)
        }
        AddHandler diffuseColorPanel.Click, Sub() ChooseColor(diffuseColorPanel, "Diffuse")
        AddHandler btnDiffuse.Click, Sub() ChooseColor(diffuseColorPanel, "Diffuse")
        y += 30

        ' Ambient Color
        Dim lblAmbient As New Label With {
            .Text = "Ambient Color:",
            .Location = New Point(20, y),
            .Width = 100
        }
        Dim ambientColorPanel As New Panel With {
            .Location = New Point(130, y - 2),
            .Size = New Size(50, 20),
            .BorderStyle = BorderStyle.FixedSingle,
            .Cursor = Cursors.Hand
        }
        Dim btnAmbient As New Button With {
            .Text = "...",
            .Location = New Point(185, y - 2),
            .Size = New Size(30, 20)
        }
        AddHandler ambientColorPanel.Click, Sub() ChooseColor(ambientColorPanel, "Ambient")
        AddHandler btnAmbient.Click, Sub() ChooseColor(ambientColorPanel, "Ambient")
        y += 30

        ' Specular Color
        Dim lblSpecular As New Label With {
            .Text = "Specular Color:",
            .Location = New Point(20, y),
            .Width = 100
        }
        Dim specularColorPanel As New Panel With {
            .Location = New Point(130, y - 2),
            .Size = New Size(50, 20),
            .BorderStyle = BorderStyle.FixedSingle,
            .Cursor = Cursors.Hand
        }
        Dim btnSpecular As New Button With {
            .Text = "...",
            .Location = New Point(185, y - 2),
            .Size = New Size(30, 20)
        }
        AddHandler specularColorPanel.Click, Sub() ChooseColor(specularColorPanel, "Specular")
        AddHandler btnSpecular.Click, Sub() ChooseColor(specularColorPanel, "Specular")
        y += 30

        ' Emissive Color
        Dim lblEmissive As New Label With {
            .Text = "Emissive Color:",
            .Location = New Point(20, y),
            .Width = 100
        }
        Dim emissiveColorPanel As New Panel With {
            .Location = New Point(130, y - 2),
            .Size = New Size(50, 20),
            .BorderStyle = BorderStyle.FixedSingle,
            .Cursor = Cursors.Hand
        }
        Dim btnEmissive As New Button With {
            .Text = "...",
            .Location = New Point(185, y - 2),
            .Size = New Size(30, 20)
        }
        AddHandler emissiveColorPanel.Click, Sub() ChooseColor(emissiveColorPanel, "Emissive")
        AddHandler btnEmissive.Click, Sub() ChooseColor(emissiveColorPanel, "Emissive")
        y += 40

        ' Shininess
        Dim lblShininess As New Label With {
            .Text = "Shininess:",
            .Location = New Point(20, y),
            .Width = 100
        }
        Dim shininessTrack As New TrackBar With {
            .Location = New Point(130, y),
            .Width = 200,
            .Height = 45,
            .Minimum = 0,
            .Maximum = 256,
            .TickFrequency = 16,
            .Value = 32
        }
        Dim shininessValue As New Label With {
            .Location = New Point(340, y),
            .AutoSize = True,
            .Text = "32"
        }
        AddHandler shininessTrack.ValueChanged, Sub()
                                                    shininessValue.Text = shininessTrack.Value.ToString()
                                                    If currentMaterial IsNot Nothing AndAlso Not isUpdating Then
                                                        currentMaterial.Shininess = shininessTrack.Value
                                                        previewPanel.Invalidate()
                                                    End If
                                                End Sub
        y += 50

        ' Opacity
        Dim lblOpacity As New Label With {
            .Text = "Opacity:",
            .Location = New Point(20, y),
            .Width = 100
        }
        Dim opacityTrack As New TrackBar With {
            .Location = New Point(130, y),
            .Width = 200,
            .Height = 45,
            .Minimum = 0,
            .Maximum = 100,
            .TickFrequency = 10,
            .Value = 100
        }
        Dim opacityValue As New Label With {
            .Location = New Point(340, y),
            .AutoSize = True,
            .Text = "100%"
        }
        AddHandler opacityTrack.ValueChanged, Sub()
                                                  opacityValue.Text = $"{opacityTrack.Value}%"
                                                  If currentMaterial IsNot Nothing AndAlso Not isUpdating Then
                                                      currentMaterial.Opacity = opacityTrack.Value / 100.0F
                                                      previewPanel.Invalidate()
                                                  End If
                                              End Sub
        y += 50

        ' Reflection
        Dim lblReflection As New Label With {
            .Text = "Reflection:",
            .Location = New Point(20, y),
            .Width = 100
        }
        Dim reflectionTrack As New TrackBar With {
            .Location = New Point(130, y),
            .Width = 200,
            .Height = 45,
            .Minimum = 0,
            .Maximum = 100,
            .TickFrequency = 10,
            .Value = 0
        }
        Dim reflectionValue As New Label With {
            .Location = New Point(340, y),
            .AutoSize = True,
            .Text = "0%"
        }
        AddHandler reflectionTrack.ValueChanged, Sub()
                                                     reflectionValue.Text = $"{reflectionTrack.Value}%"
                                                     If currentMaterial IsNot Nothing AndAlso Not isUpdating Then
                                                         currentMaterial.Reflection = reflectionTrack.Value / 100.0F
                                                         previewPanel.Invalidate()
                                                     End If
                                                 End Sub

        ' Store references for updating
        diffuseColorPanel.Tag = "DiffusePanel"
        ambientColorPanel.Tag = "AmbientPanel"
        specularColorPanel.Tag = "SpecularPanel"
        emissiveColorPanel.Tag = "EmissivePanel"
        shininessTrack.Tag = "ShininessTrack"
        opacityTrack.Tag = "OpacityTrack"
        reflectionTrack.Tag = "ReflectionTrack"

        panel.Controls.AddRange({
            lblDiffuse, diffuseColorPanel, btnDiffuse,
            lblAmbient, ambientColorPanel, btnAmbient,
            lblSpecular, specularColorPanel, btnSpecular,
            lblEmissive, emissiveColorPanel, btnEmissive,
            lblShininess, shininessTrack, shininessValue,
            lblOpacity, opacityTrack, opacityValue,
            lblReflection, reflectionTrack, reflectionValue
        })

        Return panel
    End Function

    Private Function CreateMapsPanel() As Panel
        Dim panel As New Panel With {.Dock = DockStyle.Fill, .AutoScroll = True}
        Dim y As Integer = 20

        ' Diffuse Map
        Dim lblDiffuseMap As New Label With {
            .Text = "Diffuse Map:",
            .Location = New Point(20, y),
            .Width = 100
        }
        Dim txtDiffuseMap As New TextBox With {
            .Location = New Point(130, y),
            .Width = 250,
            .ReadOnly = True
        }
        Dim btnDiffuseBrowse As New Button With {
            .Text = "Browse...",
            .Location = New Point(385, y - 2),
            .Width = 70
        }
        Dim btnDiffuseClear As New Button With {
            .Text = "Clear",
            .Location = New Point(460, y - 2),
            .Width = 50
        }
        AddHandler btnDiffuseBrowse.Click, Sub() BrowseTexture(txtDiffuseMap, "Diffuse")
        AddHandler btnDiffuseClear.Click, Sub() ClearTexture(txtDiffuseMap, "Diffuse")
        y += 30

        ' Normal Map
        Dim lblNormalMap As New Label With {
            .Text = "Normal Map:",
            .Location = New Point(20, y),
            .Width = 100
        }
        Dim txtNormalMap As New TextBox With {
            .Location = New Point(130, y),
            .Width = 250,
            .ReadOnly = True
        }
        Dim btnNormalBrowse As New Button With {
            .Text = "Browse...",
            .Location = New Point(385, y - 2),
            .Width = 70
        }
        Dim btnNormalClear As New Button With {
            .Text = "Clear",
            .Location = New Point(460, y - 2),
            .Width = 50
        }
        AddHandler btnNormalBrowse.Click, Sub() BrowseTexture(txtNormalMap, "Normal")
        AddHandler btnNormalClear.Click, Sub() ClearTexture(txtNormalMap, "Normal")
        y += 30

        ' Specular Map
        Dim lblSpecularMap As New Label With {
            .Text = "Specular Map:",
            .Location = New Point(20, y),
            .Width = 100
        }
        Dim txtSpecularMap As New TextBox With {
            .Location = New Point(130, y),
            .Width = 250,
            .ReadOnly = True
        }
        Dim btnSpecularBrowse As New Button With {
            .Text = "Browse...",
            .Location = New Point(385, y - 2),
            .Width = 70
        }
        Dim btnSpecularClear As New Button With {
            .Text = "Clear",
            .Location = New Point(460, y - 2),
            .Width = 50
        }
        AddHandler btnSpecularBrowse.Click, Sub() BrowseTexture(txtSpecularMap, "Specular")
        AddHandler btnSpecularClear.Click, Sub() ClearTexture(txtSpecularMap, "Specular")
        y += 40

        ' Bump Strength
        Dim lblBumpStrength As New Label With {
            .Text = "Bump Strength:",
            .Location = New Point(20, y),
            .Width = 100
        }
        Dim bumpStrengthNumeric As New NumericUpDown With {
            .Location = New Point(130, y),
            .Width = 100,
            .Minimum = 0,
            .Maximum = 10,
            .DecimalPlaces = 1,
            .Increment = 0.1D,
            .Value = 1D
        }
        AddHandler bumpStrengthNumeric.ValueChanged, Sub()
                                                         If currentMaterial IsNot Nothing AndAlso Not isUpdating Then
                                                             currentMaterial.BumpStrength = CSng(bumpStrengthNumeric.Value)
                                                             previewPanel.Invalidate()
                                                         End If
                                                     End Sub

        ' Store references
        txtDiffuseMap.Tag = "DiffuseMapText"
        txtNormalMap.Tag = "NormalMapText"
        txtSpecularMap.Tag = "SpecularMapText"
        bumpStrengthNumeric.Tag = "BumpStrengthNumeric"

        panel.Controls.AddRange({
            lblDiffuseMap, txtDiffuseMap, btnDiffuseBrowse, btnDiffuseClear,
            lblNormalMap, txtNormalMap, btnNormalBrowse, btnNormalClear,
            lblSpecularMap, txtSpecularMap, btnSpecularBrowse, btnSpecularClear,
            lblBumpStrength, bumpStrengthNumeric
        })

        Return panel
    End Function

    Private Function CreateTexturePreviewPanel() As Panel
        Dim panel As New Panel With {.Dock = DockStyle.Fill}

        Dim tableLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .RowCount = 2,
            .ColumnCount = 2,
            .CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
        }

        ' Diffuse texture preview
        Dim diffuseLabel As New Label With {.Text = "Diffuse", .Dock = DockStyle.Top, .TextAlign = ContentAlignment.MiddleCenter}
        diffuseTexturePanel = New Panel With {.Dock = DockStyle.Fill, .BackgroundImageLayout = ImageLayout.Zoom}
        Dim diffuseContainer As New Panel With {.Dock = DockStyle.Fill}
        diffuseContainer.Controls.Add(diffuseTexturePanel)
        diffuseContainer.Controls.Add(diffuseLabel)

        ' Normal texture preview
        Dim normalLabel As New Label With {.Text = "Normal", .Dock = DockStyle.Top, .TextAlign = ContentAlignment.MiddleCenter}
        normalTexturePanel = New Panel With {.Dock = DockStyle.Fill, .BackgroundImageLayout = ImageLayout.Zoom}
        Dim normalContainer As New Panel With {.Dock = DockStyle.Fill}
        normalContainer.Controls.Add(normalTexturePanel)
        normalContainer.Controls.Add(normalLabel)

        ' Specular texture preview
        Dim specularLabel As New Label With {.Text = "Specular", .Dock = DockStyle.Top, .TextAlign = ContentAlignment.MiddleCenter}
        specularTexturePanel = New Panel With {.Dock = DockStyle.Fill, .BackgroundImageLayout = ImageLayout.Zoom}
        Dim specularContainer As New Panel With {.Dock = DockStyle.Fill}
        specularContainer.Controls.Add(specularTexturePanel)
        specularContainer.Controls.Add(specularLabel)

        tableLayout.Controls.Add(diffuseContainer, 0, 0)
        tableLayout.Controls.Add(normalContainer, 1, 0)
        tableLayout.Controls.Add(specularContainer, 0, 1)

        panel.Controls.Add(tableLayout)
        Return panel
    End Function

    Private Sub ChooseColor(colorPanel As Panel, colorType As String)
        If currentMaterial Is Nothing Then Return

        Using colorDialog As New ColorDialog()
            Select Case colorType
                Case "Diffuse"
                    colorDialog.Color = currentMaterial.DiffuseColor
                Case "Ambient"
                    colorDialog.Color = currentMaterial.AmbientColor
                Case "Specular"
                    colorDialog.Color = currentMaterial.SpecularColor
                Case "Emissive"
                    colorDialog.Color = currentMaterial.EmissiveColor
            End Select

            If colorDialog.ShowDialog() = DialogResult.OK Then
                colorPanel.BackColor = colorDialog.Color

                Select Case colorType
                    Case "Diffuse"
                        currentMaterial.DiffuseColor = colorDialog.Color
                    Case "Ambient"
                        currentMaterial.AmbientColor = colorDialog.Color
                    Case "Specular"
                        currentMaterial.SpecularColor = colorDialog.Color
                    Case "Emissive"
                        currentMaterial.EmissiveColor = colorDialog.Color
                End Select

                previewPanel.Invalidate()
                propertyGrid.Refresh()
            End If
        End Using
    End Sub

    Private Sub BrowseTexture(textBox As TextBox, textureType As String)
        If currentMaterial Is Nothing Then Return

        Using openDialog As New OpenFileDialog()
            openDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.tga;*.dds|All Files|*.*"
            openDialog.Title = $"Select {textureType} Texture"

            If openDialog.ShowDialog() = DialogResult.OK Then
                textBox.Text = Path.GetFileName(openDialog.FileName)

                Select Case textureType
                    Case "Diffuse"
                        currentMaterial.DiffuseTexture = openDialog.FileName
                        UpdateTexturePreview(diffuseTexturePanel, openDialog.FileName)
                    Case "Normal"
                        currentMaterial.NormalTexture = openDialog.FileName
                        UpdateTexturePreview(normalTexturePanel, openDialog.FileName)
                    Case "Specular"
                        currentMaterial.SpecularTexture = openDialog.FileName
                        UpdateTexturePreview(specularTexturePanel, openDialog.FileName)
                End Select

                previewPanel.Invalidate()
            End If
        End Using
    End Sub

    Private Sub ClearTexture(textBox As TextBox, textureType As String)
        If currentMaterial Is Nothing Then Return

        textBox.Clear()

        Select Case textureType
            Case "Diffuse"
                currentMaterial.DiffuseTexture = Nothing
                diffuseTexturePanel.BackgroundImage?.Dispose()
                diffuseTexturePanel.BackgroundImage = Nothing
            Case "Normal"
                currentMaterial.NormalTexture = Nothing
                normalTexturePanel.BackgroundImage?.Dispose()
                normalTexturePanel.BackgroundImage = Nothing
            Case "Specular"
                currentMaterial.SpecularTexture = Nothing
                specularTexturePanel.BackgroundImage?.Dispose()
                specularTexturePanel.BackgroundImage = Nothing
        End Select

        previewPanel.Invalidate()
    End Sub

    Private Sub UpdateTexturePreview(panel As Panel, imagePath As String)
        If panel Is Nothing Then Return

        Try
            panel.BackgroundImage?.Dispose()
            If Not String.IsNullOrEmpty(imagePath) AndAlso File.Exists(imagePath) Then
                panel.BackgroundImage = Image.FromFile(imagePath)
            End If
        Catch ex As Exception
            MessageBox.Show($"Error loading texture: {ex.Message}", "Texture Error",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub MaterialSelected(sender As Object, e As EventArgs)
        If materialListBox.SelectedItem IsNot Nothing Then
            Dim selectedName = materialListBox.SelectedItem.ToString()
            If materials.ContainsKey(selectedName) Then
                currentMaterial = materials(selectedName)
                UpdatePropertyControls()
                propertyGrid.SelectedObject = currentMaterial
                previewPanel.Invalidate()
            End If
        End If
    End Sub

    Private Sub UpdatePropertyControls()
        If currentMaterial Is Nothing Then Return

        isUpdating = True

        Try
            ' Find the tab control first
            Dim splitContainers = Me.Controls.OfType(Of SplitContainer)()
            If splitContainers.Any() Then
                Dim mainSplitter = splitContainers.First()
                Dim rightSplitter = mainSplitter.Panel2.Controls.OfType(Of SplitContainer).FirstOrDefault()

                If rightSplitter IsNot Nothing Then
                    Dim propertyContainer = rightSplitter.Panel2.Controls.OfType(Of Panel).FirstOrDefault()
                    If propertyContainer IsNot Nothing Then
                        Dim propertyTabs = propertyContainer.Controls.OfType(Of TabControl).FirstOrDefault()

                        If propertyTabs IsNot Nothing Then
                            ' Update basic properties
                            Dim basicTab = propertyTabs.TabPages.Cast(Of TabPage).FirstOrDefault(Function(t) t.Text = "Basic")
                            If basicTab IsNot Nothing Then
                                For Each ctrl In basicTab.Controls(0).Controls
                                    Select Case ctrl.Tag?.ToString()
                                        Case "DiffusePanel"
                                            CType(ctrl, Panel).BackColor = currentMaterial.DiffuseColor
                                        Case "AmbientPanel"
                                            CType(ctrl, Panel).BackColor = currentMaterial.AmbientColor
                                        Case "SpecularPanel"
                                            CType(ctrl, Panel).BackColor = currentMaterial.SpecularColor
                                        Case "EmissivePanel"
                                            CType(ctrl, Panel).BackColor = currentMaterial.EmissiveColor
                                        Case "ShininessTrack"
                                            CType(ctrl, TrackBar).Value = CInt(currentMaterial.Shininess)
                                        Case "OpacityTrack"
                                            CType(ctrl, TrackBar).Value = CInt(currentMaterial.Opacity * 100)
                                        Case "ReflectionTrack"
                                            CType(ctrl, TrackBar).Value = CInt(currentMaterial.Reflection * 100)
                                    End Select
                                Next
                            End If

                            ' Update texture maps
                            Dim mapsTab = propertyTabs.TabPages.Cast(Of TabPage).FirstOrDefault(Function(t) t.Text = "Maps")
                            If mapsTab IsNot Nothing Then
                                For Each ctrl In mapsTab.Controls(0).Controls
                                    Select Case ctrl.Tag?.ToString()
                                        Case "DiffuseMapText"
                                            CType(ctrl, TextBox).Text = If(String.IsNullOrEmpty(currentMaterial.DiffuseTexture),
                                                                            "", Path.GetFileName(currentMaterial.DiffuseTexture))
                                        Case "NormalMapText"
                                            CType(ctrl, TextBox).Text = If(String.IsNullOrEmpty(currentMaterial.NormalTexture),
                                                                            "", Path.GetFileName(currentMaterial.NormalTexture))
                                        Case "SpecularMapText"
                                            CType(ctrl, TextBox).Text = If(String.IsNullOrEmpty(currentMaterial.SpecularTexture),
                                                                             "", Path.GetFileName(currentMaterial.SpecularTexture))
                                        Case "BumpStrengthNumeric"
                                            CType(ctrl, NumericUpDown).Value = CDec(currentMaterial.BumpStrength)
                                    End Select
                                Next
                            End If
                        End If
                    End If
                End If
            End If

            ' Update texture previews
            If diffuseTexturePanel IsNot Nothing Then
                UpdateTexturePreview(diffuseTexturePanel, currentMaterial.DiffuseTexture)
            End If
            If normalTexturePanel IsNot Nothing Then
                UpdateTexturePreview(normalTexturePanel, currentMaterial.NormalTexture)
            End If
            If specularTexturePanel IsNot Nothing Then
                UpdateTexturePreview(specularTexturePanel, currentMaterial.SpecularTexture)
            End If

        Catch ex As Exception
            ' Log error but don't crash
            Debug.WriteLine($"Error updating property controls: {ex.Message}")
        End Try

        isUpdating = False
    End Sub

    Private Sub PropertyChanged(sender As Object, e As PropertyValueChangedEventArgs)
        previewPanel.Invalidate()
    End Sub

    Private Sub ApplyChanges(sender As Object, e As EventArgs)
        ' Save current changes
        If currentMaterial IsNot Nothing Then
            MessageBox.Show("Material changes applied.", "Apply",
                          MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub CreateNewMaterial(sender As Object, e As EventArgs)
        Dim name As String = InputBox("Enter material name:", "New Material", $"Material{materials.Count + 1}")
        If Not String.IsNullOrEmpty(name) AndAlso Not materials.ContainsKey(name) Then
            ' Create the new material and add it to the dictionary
            Dim newMat = New Material(name)
            materials.Add(name, newMat)

            ' Add to listbox and select it
            materialListBox.Items.Add(name)
            materialListBox.SelectedItem = name
        ElseIf materials.ContainsKey(name) Then
            MessageBox.Show("A material with this name already exists.", "Duplicate Name",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Private Sub DeleteMaterial(sender As Object, e As EventArgs)
        If materialListBox.SelectedItem IsNot Nothing Then
            Dim selectedName = materialListBox.SelectedItem.ToString()
            If selectedName <> "Default" Then ' Prevent deleting default material
                If MessageBox.Show($"Delete material '{selectedName}'?", "Confirm Delete",
                                 MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                    materials.Remove(selectedName)
                    materialListBox.Items.Remove(selectedName)
                    If materialListBox.Items.Count > 0 Then
                        materialListBox.SelectedIndex = 0
                    End If
                End If
            Else
                MessageBox.Show("Cannot delete the default material.", "Delete Material",
                              MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        End If
    End Sub

    Private Sub CloneMaterial(sender As Object, e As EventArgs)
        If currentMaterial IsNot Nothing Then
            Dim newName = InputBox($"Enter name for cloned material:", "Clone Material",
                                 $"{currentMaterial.Name}_Copy")
            If Not String.IsNullOrEmpty(newName) AndAlso Not materials.ContainsKey(newName) Then
                Dim clonedMat = currentMaterial.Clone()
                clonedMat.Name = newName
                materials.Add(newName, clonedMat)
                materialListBox.Items.Add(newName)
                materialListBox.SelectedItem = newName
            End If
        End If
    End Sub

    Private Sub SaveMaterial(sender As Object, e As EventArgs)
        If currentMaterial IsNot Nothing Then
            Using saveDialog As New SaveFileDialog()
                saveDialog.Filter = "Material Files (*.mat)|*.mat|JSON Files (*.json)|*.json"
                saveDialog.FileName = currentMaterial.Name
                saveDialog.InitialDirectory = materialLibraryPath

                If saveDialog.ShowDialog() = DialogResult.OK Then
                    Try
                        If Not Directory.Exists(Path.GetDirectoryName(saveDialog.FileName)) Then
                            Directory.CreateDirectory(Path.GetDirectoryName(saveDialog.FileName))
                        End If

                        Dim json = JsonConvert.SerializeObject(currentMaterial, Formatting.Indented)
                        File.WriteAllText(saveDialog.FileName, json)

                        MessageBox.Show($"Material saved successfully.", "Save Material",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Catch ex As Exception
                        MessageBox.Show($"Error saving material: {ex.Message}", "Save Error",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End Try
                End If
            End Using
        End If
    End Sub

    Private Sub LoadMaterial(sender As Object, e As EventArgs)
        Using openDialog As New OpenFileDialog()
            openDialog.Filter = "Material Files (*.mat)|*.mat|JSON Files (*.json)|*.json|All Files (*.*)|*.*"
            openDialog.InitialDirectory = materialLibraryPath

            If openDialog.ShowDialog() = DialogResult.OK Then
                Try
                    Dim json = File.ReadAllText(openDialog.FileName)
                    Dim loadedMat = JsonConvert.DeserializeObject(Of Material)(json)

                    If loadedMat IsNot Nothing Then
                        ' Check for duplicate names
                        Dim originalName = loadedMat.Name
                        Dim counter = 1
                        While materials.ContainsKey(loadedMat.Name)
                            loadedMat.Name = $"{originalName}_{counter}"
                            counter += 1
                        End While

                        materials.Add(loadedMat.Name, loadedMat)
                        materialListBox.Items.Add(loadedMat.Name)
                        materialListBox.SelectedItem = loadedMat.Name
                    End If
                Catch ex As Exception
                    MessageBox.Show($"Error loading material: {ex.Message}", "Load Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub

    Private Sub ExportMaterialLibrary(sender As Object, e As EventArgs)
        Using saveDialog As New SaveFileDialog()
            saveDialog.Filter = "Material Library (*.matlib)|*.matlib|JSON Library (*.json)|*.json"
            saveDialog.FileName = "MaterialLibrary"

            If saveDialog.ShowDialog() = DialogResult.OK Then
                Try
                    Dim libraryData As New MaterialLibrary() With {
                        .Materials = materials.Values.ToList()
                    }

                    Dim json = JsonConvert.SerializeObject(libraryData, Formatting.Indented)
                    File.WriteAllText(saveDialog.FileName, json)

                    MessageBox.Show($"Material library exported successfully." & vbCrLf &
                                  $"Total materials: {materials.Count}", "Export Library",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information)
                Catch ex As Exception
                    MessageBox.Show($"Error exporting library: {ex.Message}", "Export Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub

    Private Sub DrawPreview(sender As Object, e As PaintEventArgs)
        ' Enhanced preview with multiple shapes
        Dim g As Graphics = e.Graphics
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        g.Clear(Color.FromArgb(40, 40, 40))

        If currentMaterial Is Nothing Then Return

        Dim center As New Point(previewPanel.Width \ 2, previewPanel.Height \ 2)
        Dim radius As Integer = Math.Min(previewPanel.Width, previewPanel.Height) \ 3

        ' Draw preview shape based on material properties
        DrawPreviewSphere(g, center, radius)

        ' Draw material name
        Using font As New Font("Arial", 10, FontStyle.Bold)
            Dim textSize = g.MeasureString(currentMaterial.Name, font)
            g.DrawString(currentMaterial.Name, font, Brushes.White,
                        center.X - textSize.Width / 2, previewPanel.Height - 30)
        End Using
    End Sub

    Private Sub DrawPreviewSphere(g As Graphics, center As Point, radius As Integer)
        Using path As New Drawing2D.GraphicsPath()
            path.AddEllipse(center.X - radius, center.Y - radius, radius * 2, radius * 2)

            Using brush As New Drawing2D.PathGradientBrush(path)
                ' Apply material colors
                Dim litColor = BlendColors(currentMaterial.DiffuseColor, currentMaterial.SpecularColor,
                                         currentMaterial.Shininess / 256.0F)

                brush.CenterColor = Color.FromArgb(
                    CInt(currentMaterial.Opacity * 255),
                    litColor
                )

                brush.SurroundColors = {Color.FromArgb(
                    CInt(currentMaterial.Opacity * 255),
                    BlendColors(currentMaterial.AmbientColor, Color.Black, 0.7F)
                )}

                brush.FocusScales = New PointF(0.3F, 0.3F)
                brush.CenterPoint = New PointF(center.X - radius * 0.3F, center.Y - radius * 0.3F)

                g.FillPath(brush, path)
            End Using

            ' Draw specular highlight
            If currentMaterial.Shininess > 0 Then
                Dim highlightSize = CInt(radius * 0.4F * (currentMaterial.Shininess / 256.0F))
                Dim highlightPos = New Point(center.X - radius \ 3, center.Y - radius \ 3)

                Using highlightPath As New Drawing2D.GraphicsPath()
                    highlightPath.AddEllipse(highlightPos.X, highlightPos.Y, highlightSize, highlightSize)

                    Using highlightBrush As New Drawing2D.PathGradientBrush(highlightPath)
                        highlightBrush.CenterColor = Color.FromArgb(
                            CInt(200 * (currentMaterial.Shininess / 256.0F)),
                            currentMaterial.SpecularColor
                        )
                        highlightBrush.SurroundColors = {Color.Transparent}
                        highlightBrush.FocusScales = New PointF(0.5F, 0.5F)

                        g.FillPath(highlightBrush, highlightPath)
                    End Using
                End Using
            End If

            ' Add emissive glow if present
            If currentMaterial.EmissiveColor.GetBrightness() > 0 Then
                Using glowBrush As New Drawing2D.PathGradientBrush(path)
                    glowBrush.CenterColor = Color.FromArgb(50, currentMaterial.EmissiveColor)
                    glowBrush.SurroundColors = {Color.Transparent}
                    g.FillPath(glowBrush, path)
                End Using
            End If

            ' Draw rim if reflective
            If currentMaterial.Reflection > 0 Then
                Using pen As New Pen(Color.FromArgb(CInt(100 * currentMaterial.Reflection), Color.White), 2)
                    g.DrawEllipse(pen, center.X - radius, center.Y - radius, radius * 2, radius * 2)
                End Using
            End If
        End Using
    End Sub

    Private Function BlendColors(color1 As Color, color2 As Color, ratio As Single) As Color
        ratio = Math.Max(0, Math.Min(1, ratio))
        Return Color.FromArgb(
            CInt(color1.R * (1 - ratio) + color2.R * ratio),
            CInt(color1.G * (1 - ratio) + color2.G * ratio),
            CInt(color1.B * (1 - ratio) + color2.B * ratio)
        )
    End Function

    Private Sub LoadDefaultMaterials()
        ' Create default materials with proper initialization
        Dim defaultMat = New Material("Default") With {
            .DiffuseColor = Color.Gray,
            .AmbientColor = Color.FromArgb(64, 64, 64),
            .SpecularColor = Color.White,
            .Shininess = 32.0F
        }

        Dim metalMat = New Material("Metal") With {
            .DiffuseColor = Color.Silver,
            .AmbientColor = Color.FromArgb(40, 40, 40),
            .SpecularColor = Color.White,
            .Shininess = 128.0F,
            .Reflection = 0.8F
        }

        Dim glassMat = New Material("Glass") With {
            .DiffuseColor = Color.FromArgb(100, 200, 200, 255),
            .SpecularColor = Color.White,
            .Shininess = 256.0F,
            .Opacity = 0.3F,
            .Reflection = 0.9F,
            .Refraction = 1.5F
        }

        Dim woodMat = New Material("Wood") With {
            .DiffuseColor = Color.SaddleBrown,
            .AmbientColor = Color.FromArgb(40, 20, 10),
            .SpecularColor = Color.FromArgb(100, 100, 100),
            .Shininess = 16.0F
        }

        Dim plasticMat = New Material("Plastic") With {
            .DiffuseColor = Color.Red,
            .AmbientColor = Color.FromArgb(50, 0, 0),
            .SpecularColor = Color.FromArgb(200, 200, 200),
            .Shininess = 64.0F
        }

        ' Clear any existing materials
        materials.Clear()
        materialListBox.Items.Clear()

        ' Add to dictionary
        materials.Add("Default", defaultMat)
        materials.Add("Metal", metalMat)
        materials.Add("Glass", glassMat)
        materials.Add("Wood", woodMat)
        materials.Add("Plastic", plasticMat)

        ' Update list
        For Each mat In materials.Keys.OrderBy(Function(k) k)
            materialListBox.Items.Add(mat)
        Next

        ' Select the first material
        If materialListBox.Items.Count > 0 Then
            materialListBox.SelectedIndex = 0
        End If
    End Sub

    ' Add method to get selected material
    Public ReadOnly Property SelectedMaterial As Material
        Get
            Return currentMaterial
        End Get
    End Property

    ' Add method to get all materials
    Public ReadOnly Property MaterialLibrary As Dictionary(Of String, Material)
        Get
            Return materials
        End Get
    End Property
End Class