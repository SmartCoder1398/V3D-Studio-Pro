Imports System.Drawing
Imports System.Windows.Forms

Public Class AboutDialog
    Inherits Form

    Public Sub New()
        Me.Text = "About V3D Studio Pro"
        Me.Size = New Size(400, 300)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False

        Dim lblTitle As New Label With {
            .Text = "V3D Studio Pro",
            .Location = New System.Drawing.Point(50, 50),
            .AutoSize = True,
            .Font = New Font("Arial", 14, FontStyle.Bold)
        }

        Dim lblVersion As New Label With {
            .Text = "Version 1.0.0",
            .Location = New System.Drawing.Point(50, 80),
            .AutoSize = True
        }

        Dim lblCopyright As New Label With {
            .Text = "Copyright © 2024",
            .Location = New System.Drawing.Point(50, 110),
            .AutoSize = True
        }

        Dim lblDescription As New Label With {
            .Text = "Professional 3D Modeling and Animation Software",
            .Location = New System.Drawing.Point(50, 140),
            .AutoSize = True
        }

        Dim btnOK As New Button With {
            .Text = "OK",
            .Location = New System.Drawing.Point(160, 200),
            .Size = New Size(80, 30),
            .DialogResult = DialogResult.OK
        }

        Me.Controls.AddRange({lblTitle, lblVersion, lblCopyright, lblDescription, btnOK})
        Me.AcceptButton = btnOK
    End Sub
End Class