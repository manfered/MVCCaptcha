(function () {

    var captchaImageID = $('#captchaImageID');
    var imgRefreshId = $('#imgRefreshId');
    var CaptchaUserInput = $('#CaptchaUserInput');

    imgRefreshId.click(function () {
        refreshCaptchaFunc();
    });

    function refreshCaptchaFunc() {
        d = new Date();
        captchaImageID.attr('src', '/Home/GenerateCaptcha/' + d.getTime());

        CaptchaUserInput.val('');
    }

    $('#form0').ajaxForm({
        beforeSerialize: function () {

        },
        success: function (d) {

        },
        complete: function (xhr) {

            var obj = jQuery.parseJSON(xhr.responseText);

            alert(obj.Data);
            alert("captcha string was = " + obj.captchaStr);
            alert("you entered = " + obj.CaptchaUserInput);

            refreshCaptchaFunc();
        }
    });


})();