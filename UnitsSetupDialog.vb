Imports System.Drawing
Imports System.Windows.Forms

Public Class UnitsSetupDialog
    Inherits Form

    Private systemUnitCombo As ComboBox
    Private displayUnitCombo As ComboBox
    Private customValueNumeric As NumericUpDown

    Public Sub New()
        Me.Text = "Units Setup"
        Me.Size = New Size(400, 300)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        InitializeComponents()
    End Sub

    Private Sub InitializeComponents()
        Dim lblSystem As New Label With {
            .Text = "System Unit Scale:",
            .Location = New System.Drawing.Point(20, 20),
            .AutoSize = True
        }

        systemUnitCombo = New ComboBox With {
            .Location = New System.Drawing.Point(20, 45),
            .Width = 200,
            .DropDownStyle = ComboBoxStyle.DropDownList
        }
        systemUnitCombo.Items.AddRange({"Inches", "Feet", "Millimeters", "Centimeters", "Meters"})
        systemUnitCombo.SelectedIndex = 2

        Dim lblDisplay As New Label With {
            .Text = "Display Unit Scale:",
            .Location = New System.Drawing.Point(20, 90),
            .AutoSize = True
        }

        displayUnitCombo = New ComboBox With {
            .Location = New System.Drawing.Point(20, 115),
            .Width = 200,
            .DropDownStyle = ComboBoxStyle.DropDownList
        }
        displayUnitCombo.Items.AddRange({"Generic Units", "Inches", "Feet", "Millimeters", "Centimeters", "Meters"})
        displayUnitCombo.SelectedIndex = 0

        Dim btnOK As New Button With {
            .Text = "OK",
            .Location = New System.Drawing.Point(200, 220),
            .DialogResult = DialogResult.OK
        }

        Dim btnCancel As New Button With {
            .Text = "Cancel",
            .Location = New System.Drawing.Point(280, 220),
            .DialogResult = DialogResult.Cancel
        }

        Me.Controls.AddRange({lblSystem, systemUnitCombo, lblDisplay, displayUnitCombo, btnOK, btnCancel})
        Me.AcceptButton = btnOK
        Me.CancelButton = btnCancel
    End Sub
End Class