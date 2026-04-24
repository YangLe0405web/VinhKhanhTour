window.downloadFileFromBlob = (filename, content, contentType) => {
    const blob = new Blob([content], { type: contentType });
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = filename ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}
