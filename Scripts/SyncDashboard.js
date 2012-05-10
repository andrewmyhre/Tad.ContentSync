$(document).ready(function () {
    var _remoteUrl;
    $('#inspect').dialog({ autoOpen: false, modal: true, width: '75%', height: 700 });

    $('#inspect .show-preview').click(showPreview);

    $('#inspect .show-diff').click(showDiff);

    $('.show-inspect').click(function (element) {

        var left = null, right = null;
        var diffLeft, diffRight;
        if ($(element.target).hasClass('right')) {
            right = $(element.target).siblings('.inspection');
            left = $('#' + $(right).attr('data-localid'));
            diffLeft = left.children('div.xml').html();
            diffRight = right.children('div.xml').html();
        } else if ($(element.target).hasClass('left')) {
            left = $(element.target).siblings('.inspection');
        }
        var leftAvailable = left != null && left.length != 0;
        var rightAvailable = right != null && right.length != 0;

        if (leftAvailable && !rightAvailable) {
            diffLeft = left.children('div.xml').html();
            diffRight = diffLeft;
            $('#inspect .preview-left').attr("src", "/en/Admin/ContentSync/Preview/" + left.attr('id'));
        } else if (!leftAvailable && rightAvailable) {
            diffRight = right.children('div.xml').html();
            diffLeft = diffRight;
            $('#inspect .preview-right').attr("src", "http://192.168.111.93/en/Admin/ContentSync/Preview/" + right.attr('id'));
        }
        else if (leftAvailable && rightAvailable) {
            diffLeft = left.children('div.xml').html();
            diffRight = right.children('div.xml').html();
            $('#inspect .preview-left').attr("src", "/en/Admin/ContentSync/Preview/" + left.attr('id'));
            $('#inspect .preview-right').attr("src", "http://192.168.111.93/en/Admin/ContentSync/Preview/" + right.attr('id'));
        }
        var diff = prettydiff({ source: diffLeft, diff: diffRight, mode: 'diff', lang: 'auto', diffview: 'inline'/*'sidebyside*/, sourcelabel: 'Local Version', difflabel: 'Remote Version' });
        $('#inspect .diff-view').html(diff[1]);
        $('#inspect .diff-view').append(diff[0]);

        if (left != null && left.length > 0) {

        }

        showPreview();
        $('#inspect').dialog("open");
        return false;
    });

    function init(remoteUrl) {
        _remoteUrl = remoteUrl;
    }

    function showPreview() {
        $('a.show-preview').parent().addClass('selected');
        $('a.show-diff').parent().removeClass('selected');
        $('#inspect .diff-view').hide();
        $('#inspect .preview-left').show();
        $('#inspect .preview-right').show();
    }
    function showDiff() {
        $('a.show-preview').parent().removeClass('selected');
        $('a.show-diff').parent().addClass('selected');
        $('#inspect .diff-view').show();
        $('#inspect .preview-left').hide();
        $('#inspect .preview-right').hide();

    }

});


