foreach (var attachment in message.Attachments) {
	var fileName = attachment.FileName;

	using (var stream = File.Create (fileName)) {
		attachment.Content.DecodeTo (stream);
	}
}
