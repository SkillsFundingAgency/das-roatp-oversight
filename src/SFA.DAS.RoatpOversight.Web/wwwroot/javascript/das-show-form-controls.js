var dasJs = dasJs || {};

dasJs.showHideFormControlsOnChange = {
  init: function (radiosRequiredToEnable) {
    if (!document.querySelectorAll('[data-module="das-hidden"]').length || !radiosRequiredToEnable.length) return false;

    var hiddenElements = document.querySelectorAll('[data-module="das-hidden"]');
    this.handleHiddenInputs(radiosRequiredToEnable, hiddenElements)
  },

  handleHiddenInputs: function (radiosRequiredToEnable, hiddenElements) {
    function isOneChecked() {
      var oneOfRadiosIsChecked = false;

      for (var i = 0; i < radiosRequiredToEnable.length; i++) {
        var element = document.querySelector('input#' + radiosRequiredToEnable[i]);
        if (element.checked) {
          oneOfRadiosIsChecked = true;
          break;
        } else {
          oneOfRadiosIsChecked = false;
        }
      }

      for (var i = 0; i < hiddenElements.length; i++) {
        hiddenElements[i].style.display = oneOfRadiosIsChecked ? "block" : "none"
      }
    }

    isOneChecked();

    document.addEventListener("click", function (event) {
      if (event.target.name === undefined) return false;
      isOneChecked();
    });
  }
};