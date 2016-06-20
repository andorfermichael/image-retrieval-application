$(document).on('ready', function() {
	$("#input-image-3").fileinput({
		uploadUrl: "/",
		allowedFileExtensions: ["jpg", "png"],
		resizePreference: 'height',
		maxFileCount: 1,
		resizeImage: true,
		previewFileType: "image",
        browseClass: "btn btn-success",
        browseLabel: "Pick Image",
        browseIcon: "<i class=\"glyphicon glyphicon-picture\"></i> ",
        removeClass: "btn btn-danger",
        removeLabel: "Delete",
        removeIcon: "<i class=\"glyphicon glyphicon-trash\"></i> ",
        uploadClass: "btn btn-info",
        uploadLabel: "Upload",
        uploadIcon: "<i class=\"glyphicon glyphicon-upload\"></i> "
	}).on('filepreupload', function() {
		$('#kv-success-box').html('');
	}).on('fileuploaded', function(event, data) {
		$('#kv-success-box').append(data.response.link);
		$('#kv-success-modal').modal('show');
	});
});