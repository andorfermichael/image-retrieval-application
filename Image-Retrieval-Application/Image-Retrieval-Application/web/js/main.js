$("#input-image-3").fileinput({
    uploadUrl: "/site/image-upload",
    allowedFileExtensions: ["jpg", "png", "gif"],
    maxImageWidth: 200,
    maxImageHeight: 150,
    resizePreference: 'height',
    maxFileCount: 1,
    resizeImage: true
}).on('filepreupload', function() {
    $('#kv-success-box').html('');
}).on('fileuploaded', function(event, data) {
    $('#kv-success-box').append(data.response.link);
    $('#kv-success-modal').modal('show');
});