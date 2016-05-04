Imports Microsoft.VisualBasic
Imports System.Data.SqlClient
Imports System.Data.SqlTypes
Imports System.Data

Imports System.Configuration
Imports System.IO
Imports System.Xml

Public Class dbcommon



    Private Function GetConn() As SqlConnection
        Dim strConn As String = ConfigurationManager.ConnectionStrings("connstr").ToString()
        Return New SqlConnection(strConn)
    End Function


    Public Function getDs(ByVal querystring As String) As DataSet
        Dim conn As SqlConnection = Nothing
        Dim ds As DataSet = New DataSet
        Dim da As SqlDataAdapter = Nothing
        Try
            conn = GetConn()
            conn.Open()
            da = New SqlDataAdapter(querystring, conn)
            da.SelectCommand.CommandTimeout = 180
            da.Fill(ds)
        Finally
            If conn IsNot Nothing Then conn.Close()
            If da IsNot Nothing Then da.Dispose()
        End Try
        Return ds
    End Function


    Public Function GetDS(ByVal spName As String, ByVal ParamArray parameterValues() As Object) As DataSet
        Dim ds As DataSet = Nothing
        Dim conn As SqlClient.SqlConnection = Nothing
        Try
            conn = GetConn()
            conn.Open()
            ds = SqlHelper.ExecuteDataset(conn, spName, parameterValues)
        Catch ex As Exception
            Throw New Exception("GetDS: " & spName & " " & ex.ToString)
        Finally
            If conn IsNot Nothing Then conn.Close()
        End Try
        Return ds

    End Function


    Public Function spExecute(ByVal spName As String, ByVal ParamArray parameterValues() As Object) As Boolean
        Dim myCommand As SqlCommand = Nothing
        Dim Conn As SqlConnection = Nothing
        Dim intError As Integer = 0
        Dim bretvalue As Boolean = False
        Try
            Conn = GetConn()
            Conn.Open()
            myCommand = New SqlCommand(spName, Conn)
            myCommand.CommandType = CommandType.StoredProcedure
            Dim pLength As Integer = parameterValues.GetLength(0)
            For n = 0 To pLength - 1 Step 2
                myCommand.Parameters.AddWithValue(parameterValues(n), parameterValues(n + 1))
            Next
            Dim retval As SqlParameter = New SqlParameter("RetVal", SqlDbType.Int, 4)
            retval.Direction = ParameterDirection.ReturnValue
            myCommand.Parameters.Add(retval)
            myCommand.ExecuteNonQuery()
            bretvalue = IIf(retval.Value = 0, True, False)
        Catch ex As SqlException
            Dim sqlerror As SqlError, msg As String = ""
            For Each sqlerror In ex.Errors
                '                msg &= "Sql Error Number: " & sqlerror.Number & "Msg: " & sqlerror.Message & "" & vbCrLf
                msg &= sqlerror.Message & "" & vbCrLf
            Next
            Throw New Exception(msg)
        Finally
            If Conn IsNot Nothing Then Conn.Close()
            If myCommand IsNot Nothing Then myCommand.Dispose()
        End Try
        Return bretvalue
    End Function


    Public Function spExecute(ByVal spName As String, ByRef message As String, ByVal ParamArray parameterValues() As Object) As Boolean
        Dim myCommand As SqlCommand = Nothing
        Dim Conn As SqlConnection = Nothing
        Dim intError As Integer = 0
        Dim bretvalue As Boolean = False
        Try
            Conn = GetConn()
            Conn.Open()
            myCommand = New SqlCommand(spName, Conn)
            myCommand.CommandType = CommandType.StoredProcedure
            Dim pLength As Integer = parameterValues.GetLength(0)
            For n = 0 To pLength - 1 Step 2
                myCommand.Parameters.AddWithValue(parameterValues(n), parameterValues(n + 1))
            Next
            Dim retval As SqlParameter = New SqlParameter("RetVal", SqlDbType.Int, 4)
            retval.Direction = ParameterDirection.ReturnValue
            myCommand.Parameters.Add(retval)
            myCommand.ExecuteNonQuery()
            bretvalue = IIf(retval.Value = 0, True, False)
        Catch ex As SqlException
            Dim sqlerror As SqlError, msg As String = ""
            For Each sqlerror In ex.Errors
                message &= sqlerror.Message & "" & vbCrLf
            Next
        Finally
            If Conn IsNot Nothing Then Conn.Close()
            If myCommand IsNot Nothing Then myCommand.Dispose()
        End Try
        Return bretvalue
    End Function


    Public Function spExecute2(ByVal spName As String, ByRef message As String, ByVal ParamArray parameterValues() As Object) As Integer
        Dim myCommand As SqlCommand = Nothing
        Dim Conn As SqlConnection = Nothing
        Dim intError As Integer = 0
        Dim intRows As Integer = 0
        Try
            Conn = GetConn()
            Conn.Open()
            myCommand = New SqlCommand(spName, Conn)
            myCommand.CommandType = CommandType.StoredProcedure
            Dim pLength As Integer = parameterValues.GetLength(0)
            For n = 0 To pLength - 1 Step 2
                If parameterValues(n).ToString.ToLower.Contains("xml") Then
                    Dim reader As XmlTextReader = New XmlTextReader(New StringReader(parameterValues(n + 1)))
                    Dim newxml As SqlXml = New SqlXml(reader)
                    myCommand.Parameters.AddWithValue(parameterValues(n), newxml)
                Else

                    myCommand.Parameters.AddWithValue(parameterValues(n), parameterValues(n + 1))
                End If
            Next
            Dim retval As SqlParameter = New SqlParameter("RetVal", SqlDbType.Int, 4)
            retval.Direction = ParameterDirection.ReturnValue
            myCommand.Parameters.Add(retval)
            myCommand.ExecuteNonQuery()
            intRows = retval.Value
        Catch ex As SqlException
            Dim sqlerror As SqlError, msg As String = ""
            For Each sqlerror In ex.Errors
                message &= sqlerror.Message & "" & vbCrLf
            Next
        Finally
            If Conn IsNot Nothing Then Conn.Close()
            If myCommand IsNot Nothing Then myCommand.Dispose()
        End Try
        Return intRows
    End Function


    Public Function CreateCommand(ByVal querystring As String) As Integer
        Dim Conn As SqlConnection = Nothing
        Dim myCommand As SqlCommand = Nothing
        Dim rowsAffected As Integer = 0
        Try
            Conn = GetConn()
            Conn.Open()
            myCommand = New SqlCommand(querystring, Conn)
            rowsAffected = myCommand.ExecuteNonQuery()
        Finally
            If Conn IsNot Nothing Then Conn.Close()
            If myCommand IsNot Nothing Then myCommand.Dispose()
        End Try
        Return rowsAffected
    End Function


End Class
