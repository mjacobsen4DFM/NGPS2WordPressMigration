Imports Microsoft.VisualBasic
Imports System.Data
Imports System.IO
Imports System.Xml
Imports System.Xml.Xsl
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web
Imports com.DFM.FeedHub.WordPressClient.Models

Public Class DataAuthentication

    Public Shared default_Category As String = "All Categories"

    Public Enum Fields
        destination_siteid = 1
        owstarget = 2
        owsviewsearch = 3
        owsviewreplace = 4

    End Enum


    Public Shared Function AllStop() As Boolean
        Dim ds As DataSet = GetDataSet(String.Format("SELECT [stop] FROM [ArticleDataDb].[dbo].[all_stop]"))
        Return ds.Tables(0).Rows(0).Item(0).ToString
    End Function

    Public Shared Function GetDataSet(ByVal query As String) As DataSet
        Dim DataDBdb As New DbCommon.dbcommon
        Return DataDBdb.getDs(query)
    End Function

    Public Shared Function spExecute(ByVal spName As String, ByRef message As String, ByVal ParamArray parameterValues() As Object) As Boolean
        Dim DataDBdb As New DbCommon.dbcommon
        Return DataDBdb.spExecute(spName, message, parameterValues)
    End Function

    Public Shared Function GetSectionMap(ByVal site_uid As String) As System.Data.DataSet
        Return GetDataSet(String.Format("SELECT [site_uid],[group_id],[group_name],[wp_section_list],[wp_tag_slug_list],[wp_location_list] FROM [dbo].[ngps2wp_group_map] where site_uid = '{0}'", site_uid))
    End Function

    Public Shared Function GetWPTerms(ByVal siteid As String, ByVal destination_siteid As String, ByVal taxonomy As String) As System.Data.DataSet
        Return GetDataSet(String.Format("SELECT [siteid],[destination_siteid],[id],[count],[description],[name],[slug],[taxonomy],[parent] FROM [dbo].[wp_terms] where [siteid] = '{0}' AND [destination_siteid] = '{1}' AND [taxonomy] = '{2}'", siteid, destination_siteid, taxonomy))
    End Function

    Public Shared Function ArticleResent(ByVal article_uid As String, ByVal sent As Boolean) As String
        Dim errmsg As String = ""
        Dim DataDBdb As New DbCommon.dbcommon
        DataDBdb.spExecute("ArticleResent", errmsg, "@article_uid", article_uid, "@sent", sent)
        Return errmsg
    End Function

    Public Shared Sub DeleteFromSaxoArticle(ByVal article_uid As String)
        Dim errmsg As String = ""
        Dim DataDBdb As New DbCommon.dbcommon
        DataDBdb.spExecute("deleteFromSaxoArticle", errmsg, "@article_uid", article_uid)
    End Sub

    Public Shared Function Checkout(ByVal article_uid As String, ByVal username As String) As String
        Dim errmsg As String = ""
        Dim DataDBdb As New DbCommon.dbcommon
        DataDBdb.spExecute("Checkout", errmsg, "@article_uid", article_uid, "@username", username)
        Return errmsg
    End Function

    Public Shared Function Checkin(ByVal article_uid As String, ByVal username As String, ByVal siteid As String) As String
        Dim errmsg As String = ""
        Dim DataDBdb As New DbCommon.dbcommon
        DataDBdb.spExecute("Checkin", errmsg, "@article_uid", article_uid, "@username", username)
        Return errmsg
    End Function

    Public Shared Function ArticleSelect(ByVal article_uid As String, ByVal username As String, ByVal forceState As Boolean, ByVal selectState As Boolean) As String
        Dim errmsg As String = ""
        Dim DataDBdb As New DbCommon.dbcommon
        DataDBdb.spExecute("ArticleSelect", errmsg, "@article_uid", article_uid, "@username", username, "@forceState", forceState, "@selectState", selectState)
        Return errmsg
    End Function

    Public Shared Function ArticleEdit(ByVal article_uid As String, ByVal username As String) As String
        Dim errmsg As String = ""
        Dim DataDBdb As New DbCommon.dbcommon
        DataDBdb.spExecute("ArticleEdit", errmsg, "@article_uid", article_uid, "@username", username)
        Return errmsg
    End Function

    Public Shared Sub Checkbox(ByVal article_uid As String, ByVal asset_uid As String, ByVal checked As String, ByVal siteid As String)
        Dim errmsg As String = ""
        Dim DataDBdb As New DbCommon.dbcommon
        DataDBdb.spExecute("UpdateImageTracker", errmsg, "@article_uid", article_uid, "@asset_uid", asset_uid, "@siteid", siteid, "@soft_delete", checked)
    End Sub

    Public Shared Function IsArticleLocked(ByVal article_uid As String, ByRef username As String) As Boolean
        Dim ds As DataSet = GetDataSet(String.Format("select username from article_lock where article_uid = '{0}' and locked = 'Y'", article_uid))
        If (ds.Tables(0).Rows.Count = 1) Then
            username = ds.Tables(0).Rows(0).Item(0).ToString
            Return True
        End If
        Return False
    End Function

    Public Shared Function IsSoftDelete(ByVal article_uid As String, ByVal asset_uid As String, ByVal siteid As String) As Boolean
        Dim status As Boolean = False
        Dim ds As DataSet = GetDataSet(String.Format("select soft_delete from image_tracker where asset_uid = '{0}' and siteid = '{1}' and article_uid = '{2}' ", asset_uid, siteid, article_uid))
        If (ds.Tables(0).Rows.Count = 1 AndAlso ds.Tables(0).Rows(0).Item(0).ToString.Contains("Y")) Then status = True
        Return status
    End Function

    Public Shared Function GetFreeForm(ByVal article_uid As String) As DataSet
        Dim status As Boolean = False
        Dim ds As DataSet = GetDataSet(String.Format("select html from freeform where asset_uid in ( select asset_uid from asset where asset_type = 111 and article_uid = '{0}') ", article_uid))
        Return ds
    End Function

    Public Shared Function GetArticleStoryurl(ByVal destination_siteid As String, ByVal article_uid As String) As String
        Dim storyurl As String = ""
        Dim ds As DataSet = GetDataSet(String.Format("select storyurl from saxo_article where destination_siteid = '{0}' and article_uid = '{1}' ", destination_siteid, article_uid))
        If ds.Tables(0).Rows.Count = 1 Then storyurl = ds.Tables(0).Rows(0).Item(0).ToString()
        Return storyurl
    End Function

    Public Shared Function GetArticleViewURI(ByVal destination_siteid As String, ByVal article_uid As String) As String
        Dim viewuri As String = ""
        Dim ds As DataSet = GetDataSet(String.Format("select viewuri from saxo_article where destination_siteid = '{0}' and article_uid = '{1}' ", destination_siteid, article_uid))
        If ds.Tables(0).Rows.Count = 1 Then viewuri = ds.Tables(0).Rows(0).Item(0).ToString()
        Return viewuri
    End Function

    Public Shared Function GetDestinationSiteid(ByVal siteid As String, ByVal col As Fields) As String

        Dim colname As String = col.ToString()
        Dim ds As DataSet = GetDataSet(String.Format("select {0} from saxo_pubMap where siteid = {1}", colname, siteid))
        Return ds.Tables(0).Rows(0).Item(0).ToString()
    End Function

    Public Shared Function MyCategories(ByVal siteid As String) As System.Data.DataSet
        Dim ds As DataSet = GetDataSet(String.Format("select distinct category from article where siteid = '{0}' order by category", siteid))
        ds.Tables(0).Rows.Add(default_Category)
        Return ds
    End Function

    Public Shared Function MySites() As System.Data.DataSet
        Return GetDataSet("select siteid, pubname,destination_siteid from saxo_pubMap order by pubname")
    End Function

    Public Shared Function GetImagePaths(ByVal article_uid As String) As System.Data.DataSet
        Return GetDataSet(String.Format("select imagepath, caption, asset_uid from Image where asset_uid in ( select asset_uid from asset where article_uid = '{0}' and asset_type = 108 )", article_uid))
    End Function

    Public Shared Function DoesSaxoAssetExist(ByVal destination_siteid As String, ByVal asset_uid As String) As Boolean
        Dim ds As DataSet = GetDataSet(String.Format("Select count(*) from saxo_image where asset_uid = '{0}' and destination_siteid ='{1}' and url != ''", asset_uid, destination_siteid))
        Return IIf(ds.Tables(0).Rows(0).Item(0) = 0, False, True)
    End Function

    Public Shared Function GetMediaID(ByVal destination_siteid As String, ByVal asset_uid As String) As Int32
        Dim ds As DataSet = GetDataSet(String.Format("select url as media_id from saxo_image where asset_uid = '{0}' AND destination_siteid = '{1}'", asset_uid, destination_siteid))
        If (ds.Tables(0).Rows.Count > 0) Then
            Return IIf(ds.Tables(0).Rows(0).Item(0) = "", 0, ds.Tables(0).Rows(0).Item(0))
        Else
            Return 0
        End If
    End Function

    Public Shared Function CheckMigrateArticleError(ByVal article_uid As String) As Boolean
        Dim ds As DataSet = GetDataSet(String.Format("select count(*) from MigrationError where article_uid = '{0}'", article_uid))
        Return IIf(Convert.ToInt32(ds.Tables(0).Rows(0).Item(0)) > 0, True, False)
    End Function

    'Get the error type -MSJ 20130322
    Public Shared Function GetErrorType(ByVal ds As DataSet) As Integer
        Dim item As String = ds.Tables(0).Rows(0).Item("errortype").ToString
        Return IIf(String.IsNullOrEmpty(item), 0, item)
    End Function

    'Get the image to highlight the error -MSJ 20130322
    Public Shared Function GetErrorImage(ByVal ds As DataSet) As String
        Dim item As String = ds.Tables(0).Rows(0).Item("image").ToString
        Return IIf(String.IsNullOrEmpty(item), "images/missloggederror.png", String.Format("images/{0}", item))
    End Function

    'Get the error description for the alt-text of errors -MSJ 20130322
    Public Shared Function GetErrorTypeDescription(ByVal ds As DataSet) As String
        Dim item As String = ds.Tables(0).Rows(0).Item("description").ToString
        Return IIf(String.IsNullOrEmpty(item), "Unknown error: not properly logged", item)
    End Function

    'Get the error detailed message -MSJ 20130322
    Public Shared Function GetErrorMessage(ByVal ds As DataSet) As String
        Dim item As String = ds.Tables(0).Rows(0).Item("myMessage").ToString
        Return IIf(String.IsNullOrEmpty(item), "Unknown error: not properly logged", item)
    End Function

    Public Shared Function GetMigrateArticleErrorInfo(ByVal article_uid As String) As System.Data.DataSet
        Return GetDataSet(String.Format("SELECT MigrationError.article_uid, MigrationError.errortype, MigrationErrorTypes.description, MigrationErrorTypes.image, MigrationError.myMessage FROM MigrationErrorTypes INNER JOIN MigrationErrorCategories ON MigrationErrorTypes.category = MigrationErrorCategories.category LEFT OUTER JOIN MigrationError ON MigrationErrorTypes.type = MigrationError.errortype WHERE MigrationError.article_uid = '{0}'", article_uid))
    End Function

    Public Shared Function GetMigrationErrorTypes(ByVal errorCategory As String) As System.Data.DataSet
        Dim errorholdcriteria As String = IIf(String.IsNullOrEmpty(errorCategory), String.Empty, String.Format("AND [MigrationErrorCategories].[description] = '{0}'", errorCategory))
        Return GetDataSet(String.Format("SELECT [type],[MigrationErrorTypes].[category],[title],[MigrationErrorTypes].[description],[MigrationErrorCategories].[description] categoryname,[condition],[MigrationErrorTypes].[image] FROM [ArticleDataDb].[dbo].[MigrationErrorTypes], [ArticleDataDb].[dbo].[MigrationErrorCategories] WHERE [MigrationErrorCategories].[category] = [MigrationErrorTypes].[category] {0}", errorholdcriteria))
    End Function

    Public Shared Function GetArticle(ByVal article_uid As String) As System.Data.DataSet
        Return GetDataSet(String.Format("select heading,summary,body,byline,anchor,siteid  from article where article_uid = '{0}'", article_uid))
    End Function

    Public Shared Function GetCategoryMap(ByVal site_uid As String) As System.Data.DataSet
        Return GetDataSet(String.Format("SELECT [site_uid],[category_id],[category_name],[wp_section_list],[wp_tag_slug_list],[wp_location_list] FROM [dbo].[ngps2wp_category_map] where site_uid = '{0}'", site_uid))
    End Function

    Public Shared Function GetSelectedArticles(ByVal siteid As String, ByVal errorholdtype As String) As System.Data.DataSet
        Dim errorholdcriteria As String = IIf(String.IsNullOrEmpty(errorholdtype), String.Empty, String.Format("and errorholdtype={0}", errorholdtype))
        Return GetDataSet(String.Format("select article_uid from article_override where selected = 1 and siteid = '{0}' and sent = 0 {1}", siteid, errorholdcriteria))
    End Function

    Public Shared Function ArticleState(ByVal article_uid As String) As Boolean
        Dim ds As DataSet = GetDataSet(String.Format("select selected from article_override where article_uid = '{0}' and sent = 0 AND senttimestamp is null", article_uid))
        If (ds.Tables(0).Rows.Count = 0) Then
            Return False
        Else
            Return CBool(ds.Tables(0).Rows(0).Item("selected"))
        End If
    End Function

    Public Shared Function ArticleResent(ByVal article_uid As String, ByVal stateType As String) As String
        Dim _message As String = ""
        Dim DataDBdb As New DbCommon.dbcommon
        DataDBdb.spExecute("ArticleResent", _message, "@article_uid", article_uid, "@stateType", stateType)
        Return _message
    End Function

    Public Shared Function SaveArticleProfile(ByVal article_uid As String, ByVal profileid As Integer) As String
        Dim _message As String = ""
        Dim DataDBdb As New DbCommon.dbcommon
        DataDBdb.spExecute("SaveArticleProfile", _message, "@article_uid", article_uid, "@profileid", profileid)
        Return _message
    End Function

    Public Shared Function updatepubmap(ByVal siteid As Integer, ByVal sectionmapcomplete As String) As String
        Dim _message As String = ""
        Dim DataDBdb As New DbCommon.dbcommon
        DataDBdb.spExecute("updatepubmap", _message, "@siteid", siteid, "@sectionmapcomplete", sectionmapcomplete)
        Return _message
    End Function

    Public Shared Function SaveArticle(ByVal article_uid As String, ByVal heading As String, ByVal summary As String, ByVal body As String, ByVal byline As String) As String
        Dim _message As String = ""
        Dim DataDBdb As New DbCommon.dbcommon
        DataDBdb.spExecute("SaveArticle", _message, "@article_uid", article_uid, "@heading", heading, "@summary", summary, "@body", body, "@byline", byline)
        Return _message
    End Function

    Public Shared Function DoesArticleExist(ByVal article_uid As String) As Boolean
        Dim sql As String = String.Format("Select count(*) from article where article_uid = '{0}'", article_uid)
        Dim ds As DataSet = GetDataSet(sql)
        'Console.WriteLine(sql)
        Dim records As Integer = ds.Tables(0).Rows(0).Item(0)
        Return IIf(records = 0, False, True)
    End Function

    Public Shared Function AllArticlesSelected(ByVal query As String) As Boolean
        query = query.Replace("ORDER BY", "{0} ORDER BY")
        Dim selectedCriteria As String = "AND article.article_uid NOT IN(select o.article_uid from article_override o where o.selected = 1 and o.article_uid = article.article_uid AND o.senttimestamp is null)"
        Dim ds As DataSet = GetDataSet(String.Format(query, selectedCriteria))
        If (ds.Tables(0).Rows.Count = 0) Then
            'Nothing returned = everything selected
            Return True
        Else
            'Something returned = something NOT selected
            Return False
        End If
    End Function

    Public Shared Sub StoreTerms(ByVal terms As List(Of Term))
        Dim DataDBdb As New DbCommon.dbcommon
        For Each term As Term In terms
            Dim term_in As String = String.Format("insert into wp_terms ([id],[count],[description],[name],[slug],[taxonomy],[parent]) values ({0}, {1}, '{2}', '{3}', '{4}', '{5}', {6})", term.id, term.count, term.description.Replace("'", "''"), term.name.Replace("'", "''"), term.slug, term.taxonomy, term.parent)
            DataDBdb.CreateCommand(term_in)
        Next
    End Sub

    Public Shared Function CreateCommand(ByVal querystring As String) As Integer
        Dim DataDBdb As New DbCommon.dbcommon
        Return DataDBdb.CreateCommand(querystring)
    End Function
End Class
