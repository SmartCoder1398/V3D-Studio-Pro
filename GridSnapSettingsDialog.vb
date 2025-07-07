Imports System.Drawing
Imports System.Windows.Forms

Public Class GridSnapSettingsDialog
    Inherits Form

    Private snapCheckBox As CheckBox
    Private angleSnapCheckBox As CheckBox
    Private percentSnapCheckBox As CheckBox
    Private snapValueNumeric As NumericUpDown
    Private angleValueNumeric As NumericUpDown

    Public Sub New()
        Me.Text = "Grid and Snap Settings"
        Me.Size = New Size(450, 400)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        InitializeComponents()
    End Sub

    Private Sub InitializeComponents()
        Dim tabControl As New TabControl With {
            .Dock = DockStyle.Fill
        }

        ' Snaps tab
        Dim snapsTab As New TabPage("Snaps")

        snapCheckBox = New CheckBox With {
            .Text = "Enable Grid Snap",
            .Location = New System.Drawing.Point(20, 20),
            .AutoSize = True,
            .Checked = True
        }

        Dim lblSnapValue As New Label With {
            .Text = "Grid Spacing:",
            .Location = New System.Drawing.Point(20, 50),
            .AutoSize = True
        }

        snapValueNumeric = New NumericUpDown With {
            .Location = New System.Drawing.Point(100, 48),
            .Width = 100,
            .Minimum = 0.1D,
            .Maximum = 1000,
            .DecimalPlaces = 1,
            .Value = 10
        }

        angleSnapCheckBox = New CheckBox With {
            .Text = "Enable Angle Snap",
            .Location = New System.Drawing.Point(20, 90),
            .AutoSize = True
        }

        Dim lblAngleValue As New Label With {
            .Text = "Angle:",
            .Location = New System.Drawing.Point(20, 120),
            .AutoSize = True
        }

        angleValueNumeric = New NumericUpDown With {
            .Location = New System.Drawing.Point(100, 118),
            .Width = 100,
            .Minimum = 1,
            .Maximum = 90,
            .Value = 5
        }

        snapsTab.Controls.AddRange({snapCheckBox, lblSnapValue, snapValueNumeric,
                                   angleSnapCheckBox, lblAngleValue, angleValueNumeric})

        tabControl.TabPages.Add(snapsTab)

        ' Dialog buttons
        Dim buttonPanel As New Panel With {
            .Height = 40,
            .Dock = DockStyle.Bottom
        }

        Dim btnOK As New Button With {
            .Text = "OK",
            .Location = New System.Drawing.Point(250, 8),
            .DialogResult = DialogResult.OK
        }

        Dim btnCancel As New Button With {
            .Text = "Cancel",
            .Location = New System.Drawing.Point(330, 8),
            .DialogResult = DialogResult.Cancel
        }

        buttonPanel.Controls.AddRange({btnOK, btnCancel})

        Me.Controls.Add(tabControl)
        Me.Controls.Add(buttonPanel)
    End Sub
End Class