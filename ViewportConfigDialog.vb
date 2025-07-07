Imports System.Drawing
Imports System.Windows.Forms

Public Class ViewportConfigDialog
    Inherits Form

    Private layoutCombo As ComboBox
    Private previewPanel As Panel
    Private safeFrameCheckBox As CheckBox
    Private showStatsCheckBox As CheckBox

    Public Sub New()
        Me.Text = "Viewport Configuration"
        Me.Size = New Size(500, 400)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        InitializeComponents()
    End Sub

    Private Sub InitializeComponents()
        Dim lblLayout As New Label With {
            .Text = "Layout:",
            .Location = New System.Drawing.Point(20, 20),
            .AutoSize = True
        }

        layoutCombo = New ComboBox With {
            .Location = New System.Drawing.Point(70, 18),
            .Width = 200,
            .DropDownStyle = ComboBoxStyle.DropDownList
        }
        layoutCombo.Items.AddRange({"4 Viewports", "Single Viewport", "2 Viewports Horizontal",
                                   "2 Viewports Vertical", "3 Viewports Left", "3 Viewports Right"})
        layoutCombo.SelectedIndex = 0

        ' Preview panel
        previewPanel = New Panel With {
            .Location = New System.Drawing.Point(20, 60),
            .Size = New Size(200, 150),
            .BorderStyle = BorderStyle.FixedSingle,
            .BackColor = System.Drawing.Color.DarkGray
        }

        ' Options
        safeFrameCheckBox = New CheckBox With {
            .Text = "Show Safe Frame",
            .Location = New System.Drawing.Point(250, 60),
            .AutoSize = True
        }

        showStatsCheckBox = New CheckBox With {
            .Text = "Show Statistics",
            .Location = New System.Drawing.Point(250, 90),
            .AutoSize = True
        }

        ' Dialog buttons
        Dim btnOK As New Button With {
            .Text = "OK",
            .Location = New System.Drawing.Point(300, 320),
            .DialogResult = DialogResult.OK
        }

        Dim btnCancel As New Button With {
            .Text = "Cancel",
            .Location = New System.Drawing.Point(380, 320),
            .DialogResult = DialogResult.Cancel
        }

        Me.Controls.AddRange({lblLayout, layoutCombo, previewPanel, safeFrameCheckBox,
                            showStatsCheckBox, btnOK, btnCancel})
        Me.AcceptButton = btnOK
        Me.CancelButton = btnCancel
    End Sub
End Class