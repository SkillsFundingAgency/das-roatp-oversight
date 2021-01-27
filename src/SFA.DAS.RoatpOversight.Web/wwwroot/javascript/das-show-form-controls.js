var dasJs = dasJs || {};

function nodeListForEach(nodes, callback) {
  if (window.NodeList.prototype.forEach) {
    return nodes.forEach(callback);
  }
  for (var i = 0; i < nodes.length; i++) {
    callback.call(window, nodes[i], i, nodes);
  }
}

dasJs.showHideFormControlsOnChange = {
  init: function () {
    if (
      !document.querySelectorAll('[data-module="das-reveal"]').length ||
      !document.querySelectorAll('[data-module="das-show-hide-controls"]')
        .length
    )
      return false;

    var controls = document.querySelectorAll(
      '[data-module="das-show-hide-controls"]'
    );
    var hiddenElements = document.querySelectorAll(
      '[data-module="das-reveal"]'
    );

    nodeListForEach(controls, function (control) {
      var target = control.getAttribute("data-aria-controls");
      control.setAttribute("aria-controls", target);
      control.removeAttribute("data-aria-controls");
    });

    this.handleHiddenInputs(controls, hiddenElements);
  },

  handleHiddenInputs: function (controls, hiddenElements) {
    function isOneChecked() {
      var oneChecked = false;
      nodeListForEach(controls, function (control) {
        if (control.checked) {
          oneChecked = true;
        }
      });
      return oneChecked;
    }

    nodeListForEach(hiddenElements, function (element) {
      element.classList.toggle(
        "das-reveal__conditional--hidden",
        !isOneChecked()
      );
    });

    document.addEventListener("click", function (event) {
      if (event.target.name === undefined) return false;
      nodeListForEach(hiddenElements, function (element) {
        element.classList.toggle(
          "das-reveal__conditional--hidden",
          !isOneChecked()
        );
      });
    });
  },
};
