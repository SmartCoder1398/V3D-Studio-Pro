Imports SharpDX
Imports SharpDX.Mathematics.Interop
Imports System.Runtime.InteropServices

' Define Object3D only once in this file
Public Class Object3D
    Public Property Name As String
    Public Property Type As String
    Public Property Position As SharpDX.Vector3
    Public Property Rotation As SharpDX.Vector3
    Public Property Scale As SharpDX.Vector3
    Public Property IsSelected As Boolean
    Public Property Color As SharpDX.Color4 = New SharpDX.Color4(0.8F, 0.8F, 0.8F, 1.0F)

    Public Sub New()
        Position = New SharpDX.Vector3(0, 0, 0)
        Rotation = New SharpDX.Vector3(0, 0, 0)
        Scale = New SharpDX.Vector3(1, 1, 1)
    End Sub
End Class

' Define Vector3D for compatibility
Public Structure Vector3D
    Public X As Single
    Public Y As Single
    Public Z As Single

    Public Sub New(x As Single, y As Single, z As Single)
        Me.X = x
        Me.Y = y
        Me.Z = z
    End Sub

    Public Function ToSharpDX() As SharpDX.Vector3
        Return New SharpDX.Vector3(X, Y, Z)
    End Function

    Public Shared Function FromSharpDX(v As SharpDX.Vector3) As Vector3D
        Return New Vector3D(v.X, v.Y, v.Z)
    End Function
End Structure

' Define Mesh class only once
Public Class Mesh
    Public Property VertexBuffer As SharpDX.Direct3D11.Buffer
    Public Property IndexBuffer As SharpDX.Direct3D11.Buffer
    Public Property IndexCount As Integer
End Class

' Define VertexData structure
<StructLayout(LayoutKind.Sequential)>
Public Structure VertexData
    Public Position As Vector3
    Public Normal As Vector3

    Public Sub New(pos As Vector3, norm As Vector3)
        Position = pos
        Normal = norm
    End Sub
End Structure

' Define ConstantBufferData structure
<StructLayout(LayoutKind.Sequential)>
Public Structure ConstantBufferData
    Public World As Matrix
    Public View As Matrix
    Public Projection As Matrix
    Public Color As Vector4
End Structure

' Define GridSettings class
Public Class GridSettings
    Public Property GridSpacing As Single = 10.0F
    Public Property MajorLineInterval As Integer = 10
    Public Property GridColor As System.Drawing.Color = System.Drawing.Color.DarkGray
    Public Property ShowGrid As Boolean = True
End Class