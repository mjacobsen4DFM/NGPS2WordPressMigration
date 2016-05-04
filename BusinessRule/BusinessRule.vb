Imports System.IO
Imports System.Xml
Imports System.Xml.Xsl
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Configuration
Imports UncAccess
Imports com.DFM.FeedHub
Imports com.DFM.FeedHub.WordPressClient
Imports com.DFM.FeedHub.WordPressClient.Models



Public Class BusinessRule

    Public Shared _wpCategories As List(Of Category)
    Public Shared _wpTags As List(Of Tag)
    Public Shared _wpLocation As List(Of Location)
    Public Shared _wpAuthTags As List(Of Tag)
    Public Shared _ImageCheckSQL As String


    Public Shared _NGPS_DisplayGroupList As New List(Of NGPS_DisplayGroup2WP_SectionMap)
    Public Shared _NGPS_CategoryList As New List(Of NGPS_Category2WP_TagMap)

    'replacement props
    Public Shared _wpc As WordPressClient

    Public Enum asset
        article = 102
        image = 108
        freeform = 111
        pdf = 104
    End Enum

    Public Function MigrateArticle(ByVal startDate As String, endDate As String, ByVal documents As String, ByVal temp As String, ByVal cmsData As DataRow, ByVal article_uid As String, ByVal logFlag As String, ByVal holdrules As ArrayList, ByVal ImageCheckSQL As String, Optional ByVal bypassImgages As String = "N") As Boolean
        Dim status As Boolean = False
        Dim _message As String = String.Empty
        Dim _message2 As String = String.Empty
        Dim errorType As String = String.Empty
        Dim sqlstatement As String = String.Empty
        Dim successLog As String = String.Empty
        Dim errorLog As String = String.Empty
        Dim wpApiHost As String = String.Empty
        Dim wpPostId As String = String.Empty
        Dim wpPostLocation As String = String.Empty
        Dim wpUrl As String = String.Empty
        Dim pubId As String = String.Empty
        Dim siteid As String = String.Empty
        Dim cmsname As String = String.Empty
        Dim destination_siteid As String = String.Empty
        Dim redistype As String = String.Empty
        Dim rediskey As String = String.Empty
        Dim migrationAuthorId As String = String.Empty
        Dim defaultAuthorId As String = String.Empty
        Dim defaultGuestAuthorTermId As String = String.Empty
        Dim bOverridden As Boolean = False
        Dim bSuccess As Boolean = False
        Dim wpPosted As New PostBack()
        Dim resultMap As Dictionary(Of String, Object) = New Dictionary(Of String, Object)
        _ImageCheckSQL = ImageCheckSQL

        Try
            bOverridden = IIf(ConfigurationManager.AppSettings("articleoverride").Equals("Y"), True, False)
        Catch ex As Exception
        End Try

        Try
            wpUrl = cmsData.Item("cmsUrl")
            pubId = cmsData.Item("pubId")
            siteid = cmsData.Item("siteid")
            cmsname = cmsData.Item("cmsname")
            destination_siteid = cmsData.Item("destination_siteid")
            migrationAuthorId = cmsData.Item("migrationAuthorId")
            defaultAuthorId = cmsData.Item("defaultAuthorId")
            defaultGuestAuthorTermId = cmsData.Item("defaultGuestAuthorTermId")
            redistype = cmsData.Item("redistype")
            rediskey = cmsData.Item("rediskey")
            wpApiHost = cmsData.Item("api")

            _wpc = New WordPressClient(wpApiHost, redistype, rediskey)

            successLog = String.Format("c:\temp\Migrate_{0}_{1}_{2}_Success_{3}.log", siteid, startDate, endDate, Date.Now.ToString("yyyyMMdd"))
            errorLog = String.Format("c:\temp\Migrate_{0}_{1}_{2}_Error_{3}.log", siteid, startDate, endDate, Date.Now.ToString("yyyyMMdd"))


            'Find existing article in sent table (saxo_article)
            sqlstatement = String.Format("select storyurl from saxo_article where article_uid = '{0}' and destination_siteid = '{1}'", article_uid, destination_siteid)
            Logging(article_uid, sqlstatement, logFlag)
            Dim ds3 As DataSet = DataAuthentication.DataAuthentication.GetDataSet(sqlstatement)
            Dim bPostExists As Boolean = ds3.Tables(0).Rows.Count >= 1


            DataAuthentication.DataAuthentication.spExecute("DeleteMigrationError", _message, "@article_uid", article_uid)

            sqlstatement = String.Format("select * from article where article_uid = '{0}'", article_uid)
            Logging(article_uid, sqlstatement, logFlag)
            Dim ds As DataSet = DataAuthentication.DataAuthentication.GetDataSet(sqlstatement)

            Dim articleRow As DataRow = ds.Tables(0).Rows(0)
            Dim dsCategories As DataSet
            Dim tagDsCategories As New Dictionary(Of String, String)
            'Dim dsTags As DataSet
            Dim tagDsMap As New Dictionary(Of String, String)

            'Get indicative data from WP
            If (_wpCategories Is Nothing) Then
                Console.WriteLine("Getting Categories & Groups")
                'WP Category = NGPS Section
                dsCategories = DataAuthentication.DataAuthentication.GetSectionMap(siteid)

                Dim displayGroupMap As New NGPS_DisplayGroup2WP_SectionMap()

                For Each r As DataRow In dsCategories.Tables(0).Rows
                    displayGroupMap = New NGPS_DisplayGroup2WP_SectionMap()
                    displayGroupMap.site_uid = r.Item("site_uid").ToString()
                    displayGroupMap.group_id = r.Item("group_id").ToString()
                    displayGroupMap.group_name = r.Item("group_name").ToString()
                    displayGroupMap.wp_section_list = r.Item("wp_section_list").ToString().Replace(", ", ",").Split(",").ToList()
                    displayGroupMap.wp_tag_slug_list = r.Item("wp_tag_slug_list").ToString().Replace(", ", ",").Split(",").ToList()
                    displayGroupMap.wp_location_list = r.Item("wp_location_list").ToString().Replace(", ", ",").Split(",").ToList()
                    If (Not _NGPS_DisplayGroupList.Contains(displayGroupMap)) Then _NGPS_DisplayGroupList.Add(displayGroupMap)
                Next

                'Get categories via StormFront
                Dim categoryMap As Dictionary(Of String, Object) = _wpc.getAll("posts/", "wp/v2/categories/?per_page=100")
                Dim categoryJson As String = categoryMap("body")
                _wpCategories = Category.ListFromJson(categoryJson)
                Console.WriteLine()
            End If

            If (_wpTags Is Nothing) Then
                Console.WriteLine("Getting Tags")

                'Get tags via StormFront
                Dim tagMap As Dictionary(Of String, Object) = _wpc.getAll("posts/", "wp/v2/tags/?per_page=100")
                Dim tagJson As String = tagMap("body")
                _wpTags = Tag.ListFromJson(tagJson)
                Console.WriteLine()
            End If

            If (_wpLocation Is Nothing) Then
                Console.WriteLine("Getting Location Tags")

                'Get location terms via StormFront
                Dim locMap As Dictionary(Of String, Object) = _wpc.getAll("posts/", "wp/v2/location/?per_page=100")
                Dim locJson As String = locMap("body")
                _wpLocation = Location.ListFromJson(locJson)
                Console.WriteLine()
            End If

            If (_wpAuthTags Is Nothing) Then
                Console.WriteLine("Getting Author Tags")

                'Get co-authors via StormFront
                Dim authMap As Dictionary(Of String, Object) = _wpc.get("posts/", "co-authors/v1/author-terms/?per_page=100")
                Dim authJson As String = authMap("body")
                _wpAuthTags = Tag.ListFromJson(authJson)
                Console.WriteLine()
            End If

            If (_wpCategories Is Nothing OrElse _wpTags Is Nothing OrElse _wpLocation Is Nothing OrElse _wpAuthTags Is Nothing) Then
                Console.WriteLine("One or more collections are empty: _wpCategories Is Nothing? {0} _wpTags Is Nothing? {1} _wpLocation Is Nothing? {2} _wpAuthTags Is Nothing? {3} ", _wpCategories Is Nothing, _wpTags Is Nothing, _wpLocation Is Nothing, _wpAuthTags Is Nothing)
                Return False
            End If

            'post and tags for later
            Dim wpPost As New Post()
            Dim tagList As New List(Of Integer)

            'Match article data to indicitive data
            Dim displaygroups As String = articleRow("displaygroup").ToString
            tagList = SetNGPSDisplayGroup2Tags(displaygroups)
            DataAuthentication.DataAuthentication.spExecute("insert_wp_data", _message, "@article_uid", article_uid, "@datatype", "tagList", "@data", String.Join(",", tagList), "@object", String.Empty)

            'Find authors
            Dim byline As String = articleRow("byline").ToString
            Dim email As String = articleRow("email").ToString
            Dim capTermQueue As List(Of Tag) = SetCoAuthors(article_uid, byline, email, cmsname, defaultGuestAuthorTermId)

            Dim authNames As String = ""
            For Each capTerm As Tag In capTermQueue
                authNames &= String.Format("({0})", capTerm.name)
            Next
            DataAuthentication.DataAuthentication.spExecute("insert_wp_data", _message, "@article_uid", article_uid, "@datatype", "authNames", "@data", authNames, "@object", String.Empty)

            If (bPostExists) Then
                'Article was sent, get ID and then post
                Console.Write("^")
                Return True

                wpPostId = ds3.Tables(0).Rows(0).Item(0).ToString()


                'Get existing post JSON via StormFront
                Dim postMap As Dictionary(Of String, Object) = _wpc.get("posts/", String.Format("wp/v2/posts/{0}", wpPostId))
                Dim postJson As String = postMap("body")
                wpPosted = PostBack.FromJson(postJson)
                If (wpPosted.author <> migrationAuthorId) Then
                    'This was finished. We need a very good reason to touch it
                    'Skipping
                    Console.Write("|")
                    Return status
                Else
                    'This is unfinished; fix it
                    Console.Write(">")

                    '' CREATE POST from POSTBACK
                    wpPost = New Post() With {
                        .id = wpPostId,
                        .type = wpPosted.type,
                        .title = wpPosted.title.rendered,
                        .excerpt = wpPosted.excerpt.rendered,
                        .content = wpPosted.content.rendered,
                        .author = wpPosted.author,
                        .date_gmt = fixDate(wpPosted.date_gmt),
                        .status = "publish"
                    }
                    If (wpPosted.categories IsNot Nothing) Then tagList.AddRange(wpPosted.categories)
                    If (wpPosted.tags IsNot Nothing) Then tagList.AddRange(wpPosted.tags)
                    If (wpPosted.location IsNot Nothing) Then tagList.AddRange(wpPosted.location)
                End If
            Else
                'New article
                Console.Write("+")

                '' CREATE NEW POST
                wpPost = New Post() With {
                    .type = "post",
                    .title = articleRow("heading").ToString,
                    .excerpt = articleRow("heading").ToString,
                    .content = articleRow("body").ToString,
                    .author = migrationAuthorId,
                    .date_gmt = fixDate(articleRow("startdate").ToString),
                    .status = "publish"
                }
            End If

            'Add all the tags to the appropriate terms
            wpPost.categories = tagList
            wpPost.tags = tagList
            wpPost.location = tagList

            ''POST ARTICLE
            Dim articleJson As String = wpPost.ToJSON()
            DataAuthentication.DataAuthentication.spExecute("insert_wp_data", _message, "@article_uid", article_uid, "@datatype", "articleJson", "@data", articleJson, "@object", String.Empty)

            'Post the article via StormFront
            resultMap = _wpc.post("posts/", "wp/v2/posts/" & wpPostId, articleJson)


            If (1 = 9 OrElse WebClient.isOK(CInt(resultMap("code")))) Then
                Dim articleJsonBack As String = ""

                If (1 = 9) Then
                    'Track posting
                    articleJsonBack = articleJson
                Else
                    'Track posting
                    articleJsonBack = resultMap("body")
                End If

                wpPosted = PostBack.FromJson(articleJsonBack)
                wpPostLocation = wpPosted._links.self.Item(0).href
                wpPostId = wpPosted.id
                Dim xmlPosted As String = wpPosted.ToXML()

                'Record the Article.
                'Necesarry for tracking, recovery, and troubleshooting
                'But, most immediately, for posting Images.
                DataAuthentication.DataAuthentication.spExecute("LoadSaxoArticle", _message, "@article_uid", article_uid, "@siteid", siteid, "@destination_siteid", destination_siteid, "@xmldata", xmlPosted, "@viewuri", "posted.link", "@storyurl", wpPostId)


                If _message.Length > 0 Then
                    Logging(article_uid, String.Format("stored proecedure LoadSaxoArticle table: saxo_article msg: {0}", _message), logFlag)
                    Throw New Exception(_message)
                End If
                Logging(article_uid, "stored proecedure LoadSaxoArticle table: saxo_article success", logFlag)

                ''ADD META DATA
                'Create original source canonical URL
                Dim canonical_url As String = String.Format("http://{0}/{1}/ci_{2}/{3}", articleRow("site_url").ToString.Trim("/"), articleRow("vanity_url").ToString.Trim("/"), article_uid, articleRow("seodescription").ToString.Trim("/"))
                'Build list of meta values
                Dim customFields As Dictionary(Of String, String) = New Dictionary(Of String, String)()
                customFields.Add("original_source", canonical_url)
                customFields.Add("original_byline", articleRow("byline").ToString)
                customFields.Add("discovered_authors", authNames)
                customFields.Add("original_email", articleRow("email").ToString)
                customFields.Add("original_pubdate", articleRow("startdate").ToString())
                customFields.Add("original_id", article_uid)
                customFields.Add("original_category", articleRow("category").ToString())

                ''MIGRATE IMAGES
                Dim imageMap As Dictionary(Of String, String) = PostImages(article_uid, destination_siteid, wpPostId, wpPostLocation, defaultAuthorId)
                If (CInt(imageMap.Item("sent")) > 0) Then
                    wpPost.featured_media = imageMap.Item("featured_media")
                    If (CInt(imageMap.Item("sent")) > 1) Then
                        'customFields.Add("featured_gallery", imageMap.Item("featured_gallery"))
                        customFields.Add("migrated_featured_gallery", "true")
                    End If
                    Dim articleRevisedJson As String = wpPost.ToJSON()

                    'Post the article via StormFront
                    resultMap = _wpc.post("posts/", "wp/v2/posts/" & wpPostId, articleRevisedJson)
                    DataAuthentication.DataAuthentication.spExecute("insert_wp_data", _message, "@article_uid", article_uid, "@datatype", "articleRevisedJson", "@data", articleRevisedJson, "@object", String.Empty)
                End If


                'Post meta data (including featured_gallery, if appropriate) via StormFront
                AddWpMeta(wpPostId, customFields)


                'REVISE AUTHOR/GUEST-AUTHOR (Co-Author-Plus)
                Dim capJsonFormat As String = My.Resources.wpResources.capJson
                Dim capJsonBase As String = ""
                Dim capJson As String = ""
                Dim caps As List(Of Int16) = New List(Of Int16)
                For Each capTerm As Tag In capTermQueue
                    caps.Add(capTerm.id)
                Next
                capJsonBase = String.Format(capJsonFormat, String.Join(",", caps.ToArray()))
                capJson = "{ " & capJsonBase & " }"

                'Post the author via StormFront
                resultMap = _wpc.post("posts/attributes/", "co-authors/v1/posts/" + wpPostId.ToString() + "/author-terms/", capJson)
                DataAuthentication.DataAuthentication.spExecute("insert_wp_data", _message, "@article_uid", article_uid, "@datatype", "capJson", "@data", capJson, "@object", String.Empty)
                Console.Write("a")
                'Next
            Else
                'Log errors
                Console.Write("!")
                'Console.WriteLine()
                'Console.WriteLine("Error! Code:  {0}, Message: {1}", resultMap("code"), resultMap("error"))
            End If
        Catch ex As Exception
            status = False
            _message = String.Format("Article {0} Exception {1} stack {2} ", article_uid, ex.Message, ex.StackTrace)
            DataAuthentication.DataAuthentication.spExecute("insert_replay_wp", _message2, "@article_uid", article_uid, "@post_id", wpPostId, "@asset_uid", String.Empty, "@media_id", String.Empty, "@message", _message)

            Console.WriteLine(_message)
            System.IO.File.AppendAllText(String.Format(errorLog, siteid, Date.Now.ToString("yyyyMMdd")), String.Format("{4}{4}{6}{5}taxpubid: {0}, siteid: {1}, articleid: {2}, error: {3}{4}", pubId, siteid, article_uid, ex.ToString, vbCrLf, vbTab, Date.Now.ToString))
            'Added "errorType" parameter to the MigrationErrors stored procedure, and underlying table -MSJ 20130322
            If ((ex.InnerException IsNot Nothing)) AndAlso (Not String.IsNullOrEmpty(ex.InnerException.Message)) Then
                errorType = IIf(IsNumeric(ex.InnerException.Message), ex.InnerException.Message, 0)
            Else
                errorType = 0
            End If
        End Try
        If status = False Then
            If (_message.Length = 0) Then _message = "unknown Error "
            'Added "errorType" parameter to the MigrationErrors stored procedure, and underlying table -MSJ 20130322
            DataAuthentication.DataAuthentication.spExecute("insert_replay_wp", _message2, "@article_uid", article_uid, "@post_id", wpPostId, "@asset_uid", String.Empty, "@media_id", String.Empty, "@message", _message)
            DataAuthentication.DataAuthentication.spExecute("MigrationErrors", _message2, "@article_uid", article_uid, "@myMessage", _message, "@errorType", errorType)
            System.IO.File.AppendAllText(String.Format(errorLog, siteid, Date.Now.ToString("yyyyMMdd")), String.Format("{4}{4}{6}{5}taxpubid: {0}, siteid: {1}, articleid: {2}, error: {3}{4}", pubId, siteid, article_uid, _message, vbCrLf, vbTab, Date.Now.ToString))
        End If

        If (bOverridden AndAlso status) Then
            'Mark as having been sent
            ArticleResent(article_uid, status)
        End If

        If (status) Then
            DataAuthentication.DataAuthentication.spExecute("insert_success_wp", _message, "@article_uid", article_uid, "@post_id", wpPostId, "@siteid", siteid, "@destination_siteid", destination_siteid)
            System.IO.File.AppendAllText(successLog, String.Format("{4}{4}{6}{5}taxpubid: {0}, siteid: {1}, articleid: {2}, id: {3}{4}", pubId, siteid, article_uid, wpPostId, vbCrLf, vbTab, Date.Now.ToString))
        End If
        Return status
    End Function

    Private Shared Sub Logging(ByVal article_uid As String, ByVal statement As String, ByVal logFlag As String)
        Dim message As String = ""
        If (logFlag.Equals("SUPERADMIN")) Then
            DataAuthentication.DataAuthentication.spExecute("LogStatement", message, "@article_uid", article_uid, "@statement", statement)
        End If
    End Sub

    Public Shared Function BusGetDataset(ByVal query As String) As DataSet
        Return DataAuthentication.DataAuthentication.GetDataSet(query)
    End Function

    Public Shared Function ArticleResent(ByVal article_uid As String, ByVal sent As Boolean) As String
        Dim errmsg As String = DataAuthentication.DataAuthentication.ArticleResent(article_uid, sent)
        Return errmsg
    End Function


    Public Function AllStop() As Boolean
        Return DataAuthentication.DataAuthentication.AllStop()
    End Function

    Public Shared Function DoesArticleExist(ByVal article_uid As String) As Boolean
        Return DataAuthentication.DataAuthentication.DoesArticleExist(article_uid)
    End Function

    Public Shared Function GetCMSData(ByVal siteid As Integer, ByVal destination_siteid As String) As DataRow
        Dim dataSet As DataSet = DataAuthentication.DataAuthentication.GetDataSet(String.Format("select * from cms_pubMap where siteid = {0} and destination_siteid = '{1}'", siteid, destination_siteid))
        Return dataSet.Tables(0).Rows(0)
    End Function

    Public Shared Function htmlholdrules() As ArrayList
        Dim alist As New ArrayList
        Dim sqlstatement As String = "select htmltag from htmlholdrules"
        Dim ds As DataSet = DataAuthentication.DataAuthentication.GetDataSet(sqlstatement)
        For Each dr As DataRow In ds.Tables(0).Rows
            alist.Add(dr(0).ToString)
        Next
        Return alist
    End Function

    Public Shared Function GetWordpressPubid(ByVal siteid As String, ByVal destination_siteid As String) As String
        Dim taxpubid As String = ""
        Dim sqlstatement As String = String.Format("select pubId from cms_pubMap where siteid = {0} and destination_siteid = '{1}'", siteid, destination_siteid)
        Dim ds As DataSet = DataAuthentication.DataAuthentication.GetDataSet(sqlstatement)
        If ds.Tables(0).Rows.Count = 1 Then
            taxpubid = IIf(IsDBNull(ds.Tables(0).Rows(0).Item(0)), "", ds.Tables(0).Rows(0).Item(0))
        End If
        Return taxpubid
    End Function

    Private Function PostImages(ByVal article_uid As String, ByVal destination_siteid As String, ByVal wpPostId As String, ByVal wpPostlLocation As String, ByVal authorId As String) As Dictionary(Of String, String)
        Dim resultMap As Dictionary(Of String, Object) = New Dictionary(Of String, Object)
        Dim sentMap As Dictionary(Of String, String) = New Dictionary(Of String, String)
        Dim imageList As List(Of String) = New List(Of String)
        Dim sent As Integer = 0
        Dim failures As Integer = 0
        Dim bFirst As Boolean
        Dim bFeatured As Boolean = False
        Dim media_id As Integer
        Dim mediaLocation As String = String.Empty
        Dim sqlMessage As String = String.Empty
        Dim errorMessage As String = String.Empty

        Try
            DataAuthentication.DataAuthentication.spExecute("migrationClear", sqlMessage, "@article_uid", article_uid)
            'Logging(article_uid, "Clear migrationerror table", "SUPERADMIN")
            Dim sqlFormat As String = File.ReadAllText(_ImageCheckSQL)
            Dim sqlstatement As String = String.Format(sqlFormat, destination_siteid, CInt(asset.image), article_uid)
            Dim ds As DataSet = DataAuthentication.DataAuthentication.GetDataSet(sqlstatement)
            Dim found As Integer = ds.Tables(0).Rows.Count
            Dim pubdate = "01/01/01"
            Dim i As Integer = 0
            'Logging(article_uid, sqlstatement, "SUPERADMIN")
            For Each dr As DataRow In ds.Tables(0).Rows
                Try
                    i += 1
                    bFirst = Not (bFeatured)
                    Console.Write("i")

                    'Post image via StormFront
                    resultMap = postImage(wpPostId, wpPostlLocation, authorId, dr, bFirst)

                    If (WebClient.isOK(CInt(resultMap("code")))) Then
                        mediaLocation = resultMap("location")
                        media_id = resultMap("id")
                        If (Not (bFeatured)) Then
                            bFeatured = True
                            sentMap.Add("featured_media", media_id)
                        End If
                        imageList.Add(String.Format("array( 'image' => {0} )", media_id))
                        DataAuthentication.DataAuthentication.spExecute("LoadSaxoImage", sqlMessage, "@asset_uid", dr.Item("asset_uid"), "@destination_siteid", destination_siteid, "@url", mediaLocation)
                        sent += 1
                    Else
                        media_id = 0
                        failures += 1
                        'Log errors
                        Console.Write("!")
                        'Console.WriteLine()
                        'Console.WriteLine("Error! Code: {0}, Message: {1}", resultMap("code"), resultMap("error"))
                        DataAuthentication.DataAuthentication.spExecute("MigrationErrors", sqlMessage, "@article_uid", article_uid, "@myMessage", resultMap("error"), "@errorType", CInt(resultMap("code")))
                        'DataAuthentication.DataAuthentication.spExecute("insert_replay_wp", sqlMessage, "@article_uid", article_uid, "@post_id", dr.Item("post_id"), "@asset_uid", dr.Item("asset_uid"), "@media_id", media_id, "@message", resultMap("error"))
                    End If

                Catch ex As Exception
                    errorMessage = String.Format("Exception {0} {1}", ex.Message, ex.StackTrace)
                    Console.WriteLine("Exception {0} {1}", ex.Message, ex.StackTrace)
                End Try
            Next
        Catch ex As Exception
            errorMessage = String.Format("Exception {0} {1}", ex.Message, ex.StackTrace)
            Console.WriteLine("Exception {0} {1}", ex.Message, ex.StackTrace)
        End Try

        If ((failures > 0) Or (errorMessage.Length > 0)) Then
            'Added "errorType" parameter to the MigrationErrors stored procedure, and underlying table -MSJ 20130322
            DataAuthentication.DataAuthentication.spExecute("MigrationErrors", sqlMessage, "@article_uid", article_uid, "@myMessage", errorMessage, "@errorType", 1)
        End If

        Dim imagesCSV As String = String.Join(", ", imageList)
        Dim imageMeta As String = String.Format("array( images => array( {0} ) );", imagesCSV)
        sentMap.Add("featured_gallery", imageMeta)
        sentMap.Add("sent", sent)
        sentMap.Add("failures", failures)
        Return sentMap
    End Function

    Private Function postImage(ByVal wpPostId As String, ByVal wpPostlocation As String, ByVal authorId As String, ByVal dr As DataRow, bFirst As Boolean) As Dictionary(Of String, Object)
        Dim resultMap As Dictionary(Of String, Object) = New Dictionary(Of String, Object)
        Dim jsonFormat As String = My.Resources.wpResources.imageJson
        Dim post_id As String = wpPostId                            '"0"
        Dim postlocation As String = wpPostlocation                 '"1"
        Dim name As String = setImageName(dr.Item("imagepath"))     '"2"
        Dim source As String = dr.Item("imagepath")                 '"3"
        Dim mimetype As String = setImageMime(dr.Item("imagepath")) '"4"
        Dim caption As String = cleanCaption(dr.Item("caption"))    '"5"
        Dim featured As String = bFirst                             '"6"
        Dim author As String = authorId                             '"7"
        Dim pubdate As String = fixDate(dr.Item("pubdate"))         '"8"

        source = source.Replace(" ", "%20")

        Dim jsonBase As String = String.Format(jsonFormat, post_id, postlocation, name, source, mimetype, caption, featured, author, pubdate)
        Dim imageJson = "{" & jsonBase & "}"

        'This will send the image source URL and caption (etc) to StormFront to have it pull and post the image
        resultMap = _wpc.post("posts/media/", "wp/v2/media/", imageJson)

        Return resultMap
    End Function

    Private Function setImageName(ByVal imagepath As String) As String
        Dim imageUrlParts() As String = Split(imagepath, "/")
        Return imageUrlParts(imageUrlParts.Length - 1)
    End Function

    Private Function setImageMime(ByVal imagepath As String) As String
        Dim value As String = String.Empty
        Dim imageName As String = setImageName(imagepath)
        Dim nameParts() As String = Split(imageName, ".")
        Dim ext As String = nameParts(nameParts.Length - 1)
        Dim mimes As Dictionary(Of String, String) = New Dictionary(Of String, String)
        With mimes
            .Add("jpg", "image/jpeg")
            .Add("jpeg", "image/jpeg")
            .Add("jpe", "image/jpeg")
            .Add("gif", "image/gif")
            .Add("png", "image/png")
            .Add("bmp", "image/bmp")
            .Add("tif", "image/tiff")
            .Add("tiff", "image/tiff")
        End With

        If (mimes.ContainsKey(ext)) Then
            value = mimes.Item(ext)
        Else
            value = mimes.Item("jpg")
        End If

        Return value
    End Function

    Private Function cleanCaption(ByVal caption As String) As String
        Dim value As String = String.Empty
        value = caption.Replace("""", "\""")
        Return value
    End Function

    Private Function fixDate(ByVal dateString As String) As String
        Dim rawDate As DateTime = DateTime.Parse(dateString)
        Dim isoDate As String = rawDate.ToString("yyyy-MM-ddTHH:mm:ss.000-00:00")
        Return isoDate
    End Function

    Private Sub AddWpMeta(ByVal wpPostId As String, ByVal customFields As Dictionary(Of String, String))
        Dim resultMap As Dictionary(Of String, Object) = New Dictionary(Of String, Object)
        Dim jsonFormat As String = My.Resources.wpResources.metaJson
        Dim jsonBase As String = String.Empty
        Dim json As String = String.Empty
        For Each field In customFields
            jsonBase = String.Format(jsonFormat, field.Key, field.Value)
            json = "{" & jsonBase & "}"

            'Post custom_fields (metadata) via StormFront
            resultMap = _wpc.post("posts/attributes/", "wp/v2/posts/" & wpPostId & "/meta/", json)
        Next
    End Sub

    Private Shared Function SetNGPSDisplayGroup2Tags(ByVal displayGroups As String) As List(Of Integer)
        Dim tags As List(Of Integer) = New List(Of Integer)()
        Dim catMatch As IEnumerable(Of Category)
        Dim tagMatch As IEnumerable(Of Tag)
        Dim locationMatch As IEnumerable(Of Location)
        Dim iDisplayGroupList As IEnumerable(Of NGPS_DisplayGroup2WP_SectionMap)
        Dim parentCat As Category
        Dim parentLoc As Location

        Dim displayGroupList As List(Of String) = displayGroups.Split(",").ToList()

        If (displayGroupList.Count > 0) Then
            For Each displayGroup As String In displayGroupList
                If (Not String.IsNullOrEmpty(displayGroup) AndAlso IsNumeric(displayGroup)) Then
                    iDisplayGroupList = From groupFinder In _NGPS_DisplayGroupList Where groupFinder.group_id = displayGroup
                    If (iDisplayGroupList IsNot Nothing) Then
                        For Each displayGroupItem As NGPS_DisplayGroup2WP_SectionMap In iDisplayGroupList
                            'Section
                            For Each section_name As String In displayGroupItem.wp_section_list
                                catMatch = From catFinder In _wpCategories Where catFinder.name.Trim().ToLower() = section_name.Replace("&", "&amp;").Trim().ToLower()
                                For Each categoryFound As Category In catMatch
                                    If (Not tags.Contains(categoryFound.id)) Then tags.Add(categoryFound.id)
                                    parentCat = categoryFound
                                    While parentCat.parent <> 0
                                        'This category has a parent: look it up
                                        parentCat = _wpCategories.Where(Function(cat) cat.id = parentCat.parent).ToList()(0)
                                        If (Not tags.Contains(parentCat.id)) Then tags.Add(parentCat.id)
                                    End While
                                Next
                            Next

                            'Tag
                            For Each tag_slug As String In displayGroupItem.wp_tag_slug_list
                                tagMatch = From tagFinder In _wpTags Where tagFinder.slug.Trim().ToLower() = tag_slug.Trim().ToLower()
                                For Each tagFound As Tag In tagMatch
                                    If (Not tags.Contains(tagFound.id)) Then tags.Add(tagFound.id)
                                Next
                            Next

                            'Location
                            For Each location_name As String In displayGroupItem.wp_location_list
                                locationMatch = From locationFinder In _wpLocation Where locationFinder.name.Trim().ToLower() = location_name.Trim().ToLower()
                                For Each locationFound As Location In locationMatch
                                    If (Not tags.Contains(locationFound.id)) Then tags.Add(locationFound.id)
                                    parentLoc = locationFound
                                    While parentLoc.parent <> 0
                                        'This category has a parent: look it up
                                        parentLoc = _wpLocation.Where(Function(loc) loc.id = parentLoc.parent).ToList()(0)
                                        If (Not tags.Contains(parentLoc.id)) Then tags.Add(parentLoc.id)
                                    End While
                                Next
                            Next
                        Next
                    End If
                End If
            Next
        End If

        Return tags
    End Function

    Private Shared Function SetCoAuthors(ByVal article_uid As String, ByVal byline As String, ByVal email As String, ByVal cmsname As String, ByVal defaultAuthorTermID As Integer) As List(Of Tag)
        Dim qAuthorTerms As New List(Of Tag)
        Dim tagMatch As List(Of Tag) = New List(Of Tag)
        Dim tag As New Tag()
        Dim bfound As Boolean = False
        Dim sqlstatement As String = String.Format("SELECT DISTINCT [searchname], [source] FROM [ArticleDataDb].[dbo].[ngps_byline_author_sources] WHERE [byline] = '{0}'", byline)
        Dim id As Integer = 0
        Dim searchname As String = ""
        Dim source As String = ""

        Try

            Dim dsAuthors As DataSet = BusGetDataset(sqlstatement)
            For Each drAuthor As DataRow In dsAuthors.Tables(0).Rows
                tag = New Tag()
                searchname = drAuthor.Item("searchname").ToString()
                source = drAuthor.Item("source").ToString()

                'Don't set the source as the author, unless nothing else is found (accomplished below)
                If (searchname = source) Then Continue For

                Try
                    'Match to display name as parsed from byline (fuzzy)
                    If (Not String.IsNullOrEmpty(searchname)) Then
                        tagMatch = _wpAuthTags.Where(Function(tagFinder)
                                                         Return tagFinder.description.ToLower().Contains(String.Format(" {0}", searchname.ToString.ToLower().Trim()))
                                                     End Function).ToList()
                        If (tagMatch.Count <> 0) Then
                            'Found a match!
                            If (Not qAuthorTerms.Contains(tagMatch.Item(0))) Then qAuthorTerms.Add(tagMatch.Item(0))
                            bfound = True
                        End If
                    End If

                    'If the neither source or cmsname are empty and the source is not this cmsname, then add that as an "author"
                    If (Not String.IsNullOrEmpty(source) AndAlso Not String.IsNullOrEmpty(cmsname) AndAlso source <> cmsname) Then
                        tagMatch = _wpAuthTags.Where(Function(tagFinder)
                                                         Return tagFinder.name.ToLower() = source.ToString.ToLower().Trim()
                                                     End Function).ToList()
                        If (tagMatch.Count <> 0) Then
                            'Found a match!
                            If (Not qAuthorTerms.Contains(tagMatch.Item(0))) Then qAuthorTerms.Add(tagMatch.Item(0))
                            bfound = True
                        End If
                    End If
                Catch ex As Exception
                    'Getting/setting the Guest-Author is not essential. Just log it.
                    DataAuthentication.DataAuthentication.spExecute("MigrationErrors", "", "@article_uid", article_uid, "@myMessage", ex.Message, "@errorType", 1)
                End Try

            Next
        Catch ex As Exception
            'Getting/setting the Guest-Author is not essential. Just log it.
            DataAuthentication.DataAuthentication.spExecute("MigrationErrors", "", "@article_uid", article_uid, "@myMessage", ex.Message, "@errorType", 1)
        End Try

        If (Not bfound) Then
            'Match to email argument; taken directly from migration DB
            If (Not String.IsNullOrEmpty(email)) Then
                tagMatch = _wpAuthTags.Where(Function(tagFinder)
                                                 Return tagFinder.description.ToLower().Contains(String.Format(" {0}", email.ToLower().Trim()))
                                             End Function).ToList()
                If (tagMatch.Count <> 0) Then
                    'Found a match!
                    If (Not qAuthorTerms.Contains(tagMatch.Item(0))) Then qAuthorTerms.Add(tagMatch.Item(0))
                    bfound = True
                End If
            End If
        End If

        If (Not bfound) Then
            'Match to byline source
            If (Not String.IsNullOrEmpty(source)) Then
                tagMatch = _wpAuthTags.Where(Function(tagFinder)
                                                 Return tagFinder.name.ToLower() = source.ToLower().Trim()
                                             End Function).ToList()
                If (tagMatch.Count <> 0) Then
                    'Found a match!
                    If (Not qAuthorTerms.Contains(tagMatch.Item(0))) Then qAuthorTerms.Add(tagMatch.Item(0))
                    bfound = True
                End If
            End If
        End If

        If (Not bfound) Then
            'Nothing was found; we really shouldn't get here!
            tagMatch = _wpAuthTags.Where(Function(tagFinder) tagFinder.id = defaultAuthorTermID).ToList()
            If (tagMatch.Count <> 0) Then
                'Found a match!
                If (Not qAuthorTerms.Contains(tagMatch.Item(0))) Then qAuthorTerms.Add(tagMatch.Item(0))
            End If
            DataAuthentication.DataAuthentication.spExecute("MigrationErrors", "", "@article_uid", article_uid, "@myMessage", String.Format("No authors were found for {0}", byline), "@errorType", 1)
        End If

        'Return the author list
        Return qAuthorTerms
    End Function
End Class
