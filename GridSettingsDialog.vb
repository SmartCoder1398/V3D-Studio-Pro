Imports System.Drawing
Imports System.Windows.Forms

Public Class GridSettingsDialog
    Inherits Form

    Private gridSpacingNumeric As NumericUpDown
    Private majorLinesNumeric As NumericUpDown
    Private gridColorButton As Button
    Private showGridCheck As CheckBox
    Private selectedColor As Color = Color.DarkGray

    Public Property GridSpacing As Single = 10.0F
    Public Property MajorLineInterval As Integer = 10
    Public Property GridColor As Color = Color.DarkGray
    Public Property ShowGrid As Boolean = True

    Public Sub New()
        Me.Text = "Grid Settings"
        Me.Size = New Size(400, 300)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        InitializeComponents()
    End Sub

    Private Sub InitializeComponents()
        Dim lblSpacing As New Label With {
            .Text = "Grid Spacing:",
            .Location = New Point(20, 30),
            .AutoSize = True
        }

        gridSpacingNumeric = New NumericUpDown With {
            .Location = New Point(120, 28),
            .Width = 100,
            .Minimum = 0.1D,
            .Maximum = 1000,
            .DecimalPlaces = 1,
            .Value = 10
        }

        Dim lblMajor As New Label With {
            .Text = "Major Lines Every:",
            .Location = New Point(20, 70),
            .AutoSize = True
        }

        majorLinesNumeric = New NumericUpDown With {
            .Location = New Point(120, 68),
            .Width = 100,
            .Minimum = 1,
            .Maximum = 100,
            .Value = 10
        }

        Dim lblColor As New Label With {
            .Text = "Grid Color:",
            .Location = New Point(20, 110),
            .AutoSize = True
        }

        gridColorButton = New Button With {
            .Location = New Point(120, 105),
            .Size = New Size(100, 25),
            .BackColor = selectedColor,
            .Text = "Choose..."
        }

        showGridCheck = New CheckBox With {
            .Text = "Show Grid in Viewports",
            .Location = New Point(20, 150),
            .AutoSize = True,
            .Checked = True
        }

        Dim btnOK As New Button With {
            .Text = "OK",
            .Location = New Point(200, 220),
            .DialogResult = DialogResult.OK
        }

        Dim btnCancel As New Button With {
            .Text = "Cancel",
            .Location = New Point(280, 220),
            .DialogResult = DialogResult.Cancel
        }

        Me.Controls.AddRange({lblSpacing, gridSpacingNumeric, lblMajor, majorLinesNumeric,
                            lblColor, gridColorButton, showGridCheck, btnOK, btnCancel})

        AddHandler gridColorButton.Click, AddressOf ChooseColor
        AddHandler btnOK.Click, AddressOf SaveSettings
    End Sub

    Private Sub ChooseColor(sender As Object, e As EventArgs)
        Using colorDialog As New ColorDialog()
            colorDialog.Color = selectedColor
            If colorDialog.ShowDialog() = DialogResult.OK Then
                selectedColor = colorDialog.Color
                gridColorButton.BackColor = selectedColor
            End If
        End Using
    End Sub

    Private Sub SaveSettings(sender As Object, e As EventArgs)
        GridSpacing = CSng(gridSpacingNumeric.Value)
        MajorLineInterval = CInt(majorLinesNumeric.Value)
        GridColor = selectedColor
        ShowGrid = showGridCheck.Checked
    End Sub
End Class