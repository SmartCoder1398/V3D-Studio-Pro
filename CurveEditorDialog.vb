Imports System.Drawing
Imports System.Windows.Forms

Public Class CurveEditorDialog
    Inherits Form

    Private curvePanel As Panel
    Private objectTreeView As TreeView
    Private propertyListBox As ListBox

    Public Sub New()
        Me.Text = "Curve Editor"
        Me.Size = New Size(800, 600)
        Me.StartPosition = FormStartPosition.CenterScreen
        InitializeComponents()
    End Sub

    Private Sub InitializeComponents()
        ' Create split container
        Dim mainSplitter As New SplitContainer With {
            .Dock = DockStyle.Fill,
            .Orientation = Orientation.Vertical,
            .SplitterDistance = 200
        }

        ' Left panel - Object hierarchy
        objectTreeView = New TreeView With {
            .Dock = DockStyle.Fill
        }
        objectTreeView.Nodes.Add("Scene Objects")

        ' Right panel - Curve editor
        curvePanel = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = System.Drawing.Color.FromArgb(40, 40, 40),
            .BorderStyle = BorderStyle.FixedSingle
        }

        ' Add timeline ruler
        Dim timelinePanel As New Panel With {
            .Height = 30,
            .Dock = DockStyle.Top,
            .BackColor = System.Drawing.Color.FromArgb(50, 50, 50)
        }

        mainSplitter.Panel1.Controls.Add(objectTreeView)
        mainSplitter.Panel2.Controls.Add(curvePanel)
        mainSplitter.Panel2.Controls.Add(timelinePanel)

        Me.Controls.Add(mainSplitter)

        ' Add toolbar
        Dim toolbar As New ToolStrip()
        toolbar.Items.Add("Auto Tangents")
        toolbar.Items.Add("Linear")
        toolbar.Items.Add("Step")
        Me.Controls.Add(toolbar)
    End Sub
End Class