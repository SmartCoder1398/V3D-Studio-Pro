Imports System.Drawing
Imports System.Windows.Forms

Public Class PreferencesDialog
    Inherits Form

    Private uiThemeCombo As ComboBox
    Private autoSaveCheckBox As CheckBox
    Private autoSaveIntervalNumeric As NumericUpDown
    Private undoLevelsNumeric As NumericUpDown

    Public Sub New()
        Me.Text = "Preferences"
        Me.Size = New Size(600, 500)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        InitializeComponents()
    End Sub

    Private Sub InitializeComponents()
        Dim tabControl As New TabControl With {
            .Dock = DockStyle.Fill
        }

        ' General tab
        Dim generalTab As New TabPage("General")

        Dim lblTheme As New Label With {
            .Text = "UI Theme:",
            .Location = New System.Drawing.Point(20, 20),
            .AutoSize = True
        }

        uiThemeCombo = New ComboBox With {
            .Location = New System.Drawing.Point(100, 18),
            .Width = 150,
            .DropDownStyle = ComboBoxStyle.DropDownList
        }
        uiThemeCombo.Items.AddRange({"Light", "Dark", "Blue"})
        uiThemeCombo.SelectedIndex = 1

        autoSaveCheckBox = New CheckBox With {
            .Text = "Enable Auto-Save",
            .Location = New System.Drawing.Point(20, 60),
            .AutoSize = True,
            .Checked = True
        }

        Dim lblInterval As New Label With {
            .Text = "Auto-Save Interval (minutes):",
            .Location = New System.Drawing.Point(20, 90),
            .AutoSize = True
        }

        autoSaveIntervalNumeric = New NumericUpDown With {
            .Location = New System.Drawing.Point(180, 88),
            .Width = 60,
            .Minimum = 1,
            .Maximum = 60,
            .Value = 10
        }

        generalTab.Controls.AddRange({lblTheme, uiThemeCombo, autoSaveCheckBox,
                                     lblInterval, autoSaveIntervalNumeric})

        tabControl.TabPages.Add(generalTab)

        ' Dialog buttons
        Dim buttonPanel As New Panel With {
            .Height = 40,
            .Dock = DockStyle.Bottom
        }

        Dim btnOK As New Button With {
            .Text = "OK",
            .Location = New System.Drawing.Point(400, 8),
            .DialogResult = DialogResult.OK
        }

        Dim btnCancel As New Button With {
            .Text = "Cancel",
            .Location = New System.Drawing.Point(480, 8),
            .DialogResult = DialogResult.Cancel
        }

        buttonPanel.Controls.AddRange({btnOK, btnCancel})

        Me.Controls.Add(tabControl)
        Me.Controls.Add(buttonPanel)
    End Sub
End Class