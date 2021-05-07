function imagePreview (input) {
    if (input.files && input.files[0]){
        let reader = new FileReader();
        
        reader.onload = function (e) {
            $("#image-preview").attr("src", e.target.result);
        };
        reader.readAsDataURL(input.files[0]);
    }
}