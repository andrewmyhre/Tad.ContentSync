var _remoteUrl;
$(document).ready(function () {
    $('#diff-view').dialog({ autoOpen: false, modal: true, width: '75%', height: 700 });

    $('#diff-view .show-preview').click(showPreview);

    $('#diff-view .show-diff').click(showDiff);

    $('#diff-select-for-sync').click(selectForSync);

    $('#button-synchronise-up').click(synchroniseUp);
    $('#button-synchronise-down').click(synchroniseDown);

    $('.link-show-diff').click(function (element) {

        var left = null, right = null;
        var diffLeft, diffRight;

        right = $(element.target).siblings('div.right.source');
        left = $(element.target).siblings('div.left.source');

        var leftAvailable = left != null && left.length != 0;
        var rightAvailable = right != null && right.length != 0;

        if (!leftAvailable) {
            left = $(element.target).parent().parent().siblings('div.left.source');
            leftAvailable = left != null && left.length != 0;
        }

        console.log('leftAvailable:' + leftAvailable);
        console.log('rightAvailable:' + rightAvailable);

        if (leftAvailable && !rightAvailable) {
            diffLeft = left.html();
            diffRight = diffLeft;
            $('#diff-view .preview-left').attr("src", "/en/Admin/ContentSync/Preview/" + left.parent().attr('id'));
        } else if (!leftAvailable && rightAvailable) {
            diffRight = right.html();
            diffLeft = diffRight;
            $('#diff-view .preview-right').attr("src", _remoteUrl+"/Admin/ContentSync/Preview/" + right.attr('id'));
        }
        else if (leftAvailable && rightAvailable) {
            diffLeft = left.html();
            diffRight = right.html();
            $('#diff-view .preview-left').attr("src", "/en/Admin/ContentSync/Preview/" + left.parent().attr('id'));
            $('#diff-view .preview-right').attr("src", _remoteUrl+"/Admin/ContentSync/Preview/" + right.attr('id'));
        }
        var diff = prettydiff({ source: diffLeft, diff: diffRight, mode: 'diff', lang: 'auto', diffview: 'inline'/*'sidebyside*/, sourcelabel: 'Local Version', difflabel: 'Remote Version' });
        $('#diff-view .diff-view').html(diff[1]);
        $('#diff-view .diff-view').append(diff[0]);

        var inputId = '#' + left.parent().attr('id') + '\\:\\:';
        if (rightAvailable)
            inputId += right.attr('id');

        $('#diff-select-for-sync').attr('href', inputId);
        if ($(inputId).prop('checked')) {
            $('#diff-select-for-sync').html('Unselect this content');
        } else {
            $('#diff-select-for-sync').html('Select this content');
        }

        if (left != null && left.length > 0) {

        }

        showPreview();
        $('#diff-view').dialog("open");
        return false;
    });

    function selectForSync(element) {
        var inputId = $(element.target).attr('href');
        var input = $(inputId);

        if (input != null && input.length > 0) {
            if (input.prop('checked')) {
                input.prop('checked', false);
                $(element.target).html('Select this content');
            } else {
                input.prop('checked', true);
                $(element.target).html('Unselect this content');
            }
        }
    }



    function showPreview() {
        $('a.show-preview').parent().addClass('selected');
        $('a.show-diff').parent().removeClass('selected');
        $('#diff-view .diff-view').hide();
        $('#diff-view .preview-left').show();
        $('#diff-view .preview-right').show();
    }
    function showDiff() {
        $('a.show-preview').parent().removeClass('selected');
        $('a.show-diff').parent().addClass('selected');
        $('#diff-view .diff-view').show();
        $('#diff-view .preview-left').hide();
        $('#diff-view .preview-right').hide();

    }

    function synchroniseUp() {
        $('input[name=direction]').val('up');
    }
    function synchroniseDown() {
        $('input[name=direction]').val('down');
    }
    function synchronise() {
        
    }
});


function setRemoteUrl(remoteUrl) {
    _remoteUrl = remoteUrl;
}