Imports SharpDX
Imports D3D11 = SharpDX.Direct3D11
Imports System.Runtime.InteropServices

Public Class MeshGenerator
    Private Shared meshCache As New Dictionary(Of String, Mesh)

    Public Shared Function GetMesh(type As String, device As D3D11.Device) As Mesh
        If meshCache.ContainsKey(type) Then
            Return meshCache(type)
        End If

        Dim mesh As Mesh = Nothing

        Select Case type
            Case "Box"
                mesh = CreateBox(device)
            Case "Sphere"
                mesh = CreateSphere(device)
            Case "Cylinder"
                mesh = CreateCylinder(device)
            Case "Plane"
                mesh = CreatePlane(device)
            Case "Torus"
                mesh = CreateTorus(device)
        End Select

        If mesh IsNot Nothing Then
            meshCache(type) = mesh
        End If

        Return mesh
    End Function

    Private Shared Function CreateBox(device As D3D11.Device) As Mesh
        Dim vertices As New List(Of VertexData)
        Dim indices As New List(Of Integer)

        ' Create box vertices (8 vertices)
        Dim size = 50.0F

        ' Front face
        vertices.Add(New VertexData(New Vector3(-size, -size, size), New Vector3(0, 0, 1)))
        vertices.Add(New VertexData(New Vector3(size, -size, size), New Vector3(0, 0, 1)))
        vertices.Add(New VertexData(New Vector3(size, size, size), New Vector3(0, 0, 1)))
        vertices.Add(New VertexData(New Vector3(-size, size, size), New Vector3(0, 0, 1)))

        ' Back face
        vertices.Add(New VertexData(New Vector3(-size, -size, -size), New Vector3(0, 0, -1)))
        vertices.Add(New VertexData(New Vector3(size, -size, -size), New Vector3(0, 0, -1)))
        vertices.Add(New VertexData(New Vector3(size, size, -size), New Vector3(0, 0, -1)))
        vertices.Add(New VertexData(New Vector3(-size, size, -size), New Vector3(0, 0, -1)))

        ' Define indices for triangles
        ' Front face
        indices.AddRange({0, 1, 2, 0, 2, 3})
        ' Back face
        indices.AddRange({5, 4, 7, 5, 7, 6})
        ' Top face
        indices.AddRange({3, 2, 6, 3, 6, 7})
        ' Bottom face
        indices.AddRange({4, 5, 1, 4, 1, 0})
        ' Right face
        indices.AddRange({1, 5, 6, 1, 6, 2})
        ' Left face
        indices.AddRange({4, 0, 3, 4, 3, 7})

        Return CreateMesh(device, vertices, indices)
    End Function

    Private Shared Function CreateSphere(device As D3D11.Device) As Mesh
        Dim vertices As New List(Of VertexData)
        Dim indices As New List(Of Integer)

        Dim radius = 50.0F
        Dim segments = 32
        Dim rings = 16

        ' Generate vertices
        For ring = 0 To rings
            Dim phi = MathUtil.Pi * ring / rings
            For segment = 0 To segments
                Dim theta = MathUtil.TwoPi * segment / segments

                Dim x = radius * Math.Sin(phi) * Math.Cos(theta)
                Dim y = radius * Math.Cos(phi)
                Dim z = radius * Math.Sin(phi) * Math.Sin(theta)

                Dim position = New Vector3(CSng(x), CSng(y), CSng(z))
                Dim normal = Vector3.Normalize(position)

                vertices.Add(New VertexData(position, normal))
            Next
        Next

        ' Generate indices
        For ring = 0 To rings - 1
            For segment = 0 To segments - 1
                Dim current = ring * (segments + 1) + segment
                Dim nextValue = current + segments + 1

                indices.Add(current)
                indices.Add(nextValue)
                indices.Add(current + 1)

                indices.Add(current + 1)
                indices.Add(nextValue)
                indices.Add(nextValue + 1)
            Next
        Next

        Return CreateMesh(device, vertices, indices)
    End Function

    Private Shared Function CreateCylinder(device As D3D11.Device) As Mesh
        Dim vertices As New List(Of VertexData)
        Dim indices As New List(Of Integer)

        Dim radius = 30.0F
        Dim height = 60.0F
        Dim segments = 32

        ' Generate cylinder body vertices
        For i = 0 To segments
            Dim angle = MathUtil.TwoPi * i / segments
            Dim x = radius * Math.Cos(angle)
            Dim z = radius * Math.Sin(angle)

            ' Bottom vertex
            vertices.Add(New VertexData(
                New Vector3(CSng(x), -height / 2, CSng(z)),
                New Vector3(CSng(x / radius), 0, CSng(z / radius))))

            ' Top vertex
            vertices.Add(New VertexData(
                New Vector3(CSng(x), height / 2, CSng(z)),
                New Vector3(CSng(x / radius), 0, CSng(z / radius))))
        Next

        ' Generate body indices
        For i = 0 To segments - 1
            Dim baseIndex = i * 2

            indices.Add(baseIndex)
            indices.Add(baseIndex + 2)
            indices.Add(baseIndex + 1)

            indices.Add(baseIndex + 1)
            indices.Add(baseIndex + 2)
            indices.Add(baseIndex + 3)
        Next

        Return CreateMesh(device, vertices, indices)
    End Function

    Private Shared Function CreatePlane(device As D3D11.Device) As Mesh
        Dim vertices As New List(Of VertexData)
        Dim indices As New List(Of Integer)

        Dim size = 100.0F
        Dim segments = 10
        Dim stepValue = size / segments

        ' Generate vertices
        For i = 0 To segments
            For j = 0 To segments
                Dim x = -size / 2 + i * stepValue
                Dim z = -size / 2 + j * stepValue

                vertices.Add(New VertexData(
                    New Vector3(x, 0, z),
                    Vector3.UnitY))
            Next
        Next

        ' Generate indices
        For i = 0 To segments - 1
            For j = 0 To segments - 1
                Dim topLeft = i * (segments + 1) + j
                Dim topRight = topLeft + 1
                Dim bottomLeft = topLeft + segments + 1
                Dim bottomRight = bottomLeft + 1

                indices.Add(topLeft)
                indices.Add(bottomLeft)
                indices.Add(topRight)

                indices.Add(topRight)
                indices.Add(bottomLeft)
                indices.Add(bottomRight)
            Next
        Next

        Return CreateMesh(device, vertices, indices)
    End Function

    Private Shared Function CreateTorus(device As D3D11.Device) As Mesh
        Dim vertices As New List(Of VertexData)
        Dim indices As New List(Of Integer)

        Dim outerRadius = 40.0F
        Dim innerRadius = 15.0F
        Dim sides = 24
        Dim rings = 32

        ' Generate vertices
        For i = 0 To rings
            Dim u = MathUtil.TwoPi * i / rings
            For j = 0 To sides
                Dim v = MathUtil.TwoPi * j / sides

                Dim x = (outerRadius + innerRadius * Math.Cos(v)) * Math.Cos(u)
                Dim y = innerRadius * Math.Sin(v)
                Dim z = (outerRadius + innerRadius * Math.Cos(v)) * Math.Sin(u)

                Dim position = New Vector3(CSng(x), CSng(y), CSng(z))

                ' Calculate normal
                Dim centerX = outerRadius * Math.Cos(u)
                Dim centerZ = outerRadius * Math.Sin(u)
                Dim normal = Vector3.Normalize(Vector3.Subtract(position, New Vector3(CSng(centerX), 0, CSng(centerZ))))

                vertices.Add(New VertexData(position, normal))
            Next
        Next

        ' Generate indices
        For i = 0 To rings - 1
            For j = 0 To sides - 1
                Dim current = i * (sides + 1) + j
                Dim nextValue = current + sides + 1

                indices.Add(current)
                indices.Add(nextValue)
                indices.Add(current + 1)

                indices.Add(current + 1)
                indices.Add(nextValue)
                indices.Add(nextValue + 1)
            Next
        Next

        Return CreateMesh(device, vertices, indices)
    End Function

    Private Shared Function CreateMesh(device As D3D11.Device, vertices As List(Of VertexData), indices As List(Of Integer)) As Mesh
        Dim mesh As New Mesh()

        ' Create vertex buffer
        Dim vertexBufferDesc As New D3D11.BufferDescription() With {
            .Usage = D3D11.ResourceUsage.Default,
            .SizeInBytes = Marshal.SizeOf(GetType(VertexData)) * vertices.Count,
            .BindFlags = D3D11.BindFlags.VertexBuffer,
            .CpuAccessFlags = D3D11.CpuAccessFlags.None
        }

        mesh.VertexBuffer = D3D11.Buffer.Create(device, vertices.ToArray(), vertexBufferDesc)

        ' Create index buffer
        Dim indexBufferDesc As New D3D11.BufferDescription() With {
            .Usage = D3D11.ResourceUsage.Default,
            .SizeInBytes = Marshal.SizeOf(GetType(Integer)) * indices.Count,
            .BindFlags = D3D11.BindFlags.IndexBuffer,
            .CpuAccessFlags = D3D11.CpuAccessFlags.None
        }

        mesh.IndexBuffer = D3D11.Buffer.Create(device, indices.ToArray(), indexBufferDesc)
        mesh.IndexCount = indices.Count

        Return mesh
    End Function

    Public Shared Sub DisposeMeshes()
        For Each kvp In meshCache
            kvp.Value.VertexBuffer?.Dispose()
            kvp.Value.IndexBuffer?.Dispose()
        Next
        meshCache.Clear()
    End Sub
End Class