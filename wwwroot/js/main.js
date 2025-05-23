(function ($) {
  "use strict";

  /*[ Load page ]
    ===========================================================*/
  $(".animsition").animsition({
    inClass: "fade-in",
    outClass: "fade-out",
    inDuration: 1500,
    outDuration: 800,
    linkElement: ".animsition-link",
    loading: true,
    loadingParentElement: "html",
    loadingClass: "animsition-loading-1",
    loadingInner: '<div class="loader05"></div>',
    timeout: false,
    timeoutCountdown: 5000,
    onLoadEvent: true,
    browser: ["animation-duration", "-webkit-animation-duration"],
    overlay: false,
    overlayClass: "animsition-overlay-slide",
    overlayParentElement: "html",
    transition: function (url) {
      window.location.href = url;
    },
  });

  /*[ Back to top ]
    ===========================================================*/
  var windowH = $(window).height() / 2;

  $(window).on("scroll", function () {
    if ($(this).scrollTop() > windowH) {
      $("#myBtn").css("display", "flex");
    } else {
      $("#myBtn").css("display", "none");
    }
  });

  $("#myBtn").on("click", function () {
    $("html, body").animate({ scrollTop: 0 }, 300);
  });

  /*==================================================================
    [ Fixed Header ]*/
  var headerDesktop = $(".container-menu-desktop");
  var wrapMenu = $(".wrap-menu-desktop");

  if ($(".top-bar").length > 0) {
    var posWrapHeader = $(".top-bar").height();
  } else {
    var posWrapHeader = 0;
  }

  if ($(window).scrollTop() > posWrapHeader) {
    $(headerDesktop).addClass("fix-menu-desktop");
    $(wrapMenu).css("top", 0);
  } else {
    $(headerDesktop).removeClass("fix-menu-desktop");
    $(wrapMenu).css("top", posWrapHeader - $(this).scrollTop());
  }

  $(window).on("scroll", function () {
    if ($(this).scrollTop() > posWrapHeader) {
      $(headerDesktop).addClass("fix-menu-desktop");
      $(wrapMenu).css("top", 0);
    } else {
      $(headerDesktop).removeClass("fix-menu-desktop");
      $(wrapMenu).css("top", posWrapHeader - $(this).scrollTop());
    }
  });

  /*==================================================================
    [ Menu mobile ]*/
  // Mobile menu toggle
  $(".btn-show-menu-mobile")
    .off("click")
    .on("click", function (e) {
      e.preventDefault();
      e.stopPropagation();
      $(this).toggleClass("is-active");
      $(".menu-mobile").slideToggle(400);
    });

  // Submenu toggle
  var arrowMainMenu = $(".arrow-main-menu-m");
  for (var i = 0; i < arrowMainMenu.length; i++) {
    $(arrowMainMenu[i]).on("click", function () {
      $(this).parent().find(".sub-menu-m").slideToggle();
      $(this).toggleClass("turn-arrow-main-menu-m");
    });
  }

  // Resize handler to hide mobile menu on desktop
  $(window).resize(function () {
    if ($(window).width() >= 992) {
      if ($(".menu-mobile").css("display") === "block") {
        $(".menu-mobile").css("display", "none");
        $(".btn-show-menu-mobile").removeClass("is-active");
      }
      $(".sub-menu-m").each(function () {
        if ($(this).css("display") === "block") {
          $(this).css("display", "none");
          $(".arrow-main-menu-m").removeClass("turn-arrow-main-menu-m");
        }
      });
    }
  });

  /*==================================================================
    [ Show / hide modal search ]*/
  $(".js-show-modal-search").on("click", function () {
    $(".modal-search-header").addClass("show-modal-search");
    $(this).css("opacity", "0");
  });

  $(".js-hide-modal-search").on("click", function () {
    $(".modal-search-header").removeClass("show-modal-search");
    $(".js-show-modal-search").css("opacity", "1");
  });

  $(".container-search-header").on("click", function (e) {
    e.stopPropagation();
  });

  /*==================================================================
    [ Isotope ]*/
  var $topeContainer = $(".isotope-grid");
  var $filter = $(".filter-tope-group");

  // filter items on button click
  $filter.each(function () {
    $filter.on("click", "button", function () {
      var filterValue = $(this).attr("data-filter");
      $topeContainer.isotope({ filter: filterValue });
    });
  });

  // init Isotope
  $(window).on("load", function () {
    var $grid = $topeContainer.each(function () {
      $(this).isotope({
        itemSelector: ".isotope-item",
        layoutMode: "fitRows",
        percentPosition: true,
        animationEngine: "best-available",
        masonry: {
          columnWidth: ".isotope-item",
        },
      });
    });
  });

  var isotopeButton = $(".filter-tope-group button");

  $(isotopeButton).each(function () {
    $(this).on("click", function () {
      for (var i = 0; i < isotopeButton.length; i++) {
        $(isotopeButton[i]).removeClass("how-active1");
      }

      $(this).addClass("how-active1");
    });
  });

  /*==================================================================
    [ Filter / Search product ]*/
  $(".js-show-filter").on("click", function () {
    $(this).toggleClass("show-filter");
    $(".panel-filter").slideToggle(400);

    if ($(".js-show-search").hasClass("show-search")) {
      $(".js-show-search").removeClass("show-search");
      $(".panel-search").slideUp(400);
    }
  });

  $(".js-show-search").on("click", function () {
    $(this).toggleClass("show-search");
    $(".panel-search").slideToggle(400);

    if ($(".js-show-filter").hasClass("show-filter")) {
      $(".js-show-filter").removeClass("show-filter");
      $(".panel-filter").slideUp(400);
    }
  });

  /*==================================================================
    [ Cart ]*/
  $(".js-show-cart").on("click", function () {
    $(".js-panel-cart").addClass("show-header-cart");
  });

  $(".js-hide-cart").on("click", function () {
    $(".js-panel-cart").removeClass("show-header-cart");
  });

  /*==================================================================
    [ Cart ]*/
  $(".js-show-sidebar").on("click", function () {
    $(".js-sidebar").addClass("show-sidebar");
  });

  $(".js-hide-sidebar").on("click", function () {
    $(".js-sidebar").removeClass("show-sidebar");
  });

  /*==================================================================
    [ +/- num product ]*/
  $(".btn-num-product-down").on("click", function () {
    var numProduct = Number($(this).next().val());
    if (numProduct > 0)
      $(this)
        .next()
        .val(numProduct - 1);
  });

  $(".btn-num-product-up").on("click", function () {
    var numProduct = Number($(this).prev().val());
    $(this)
      .prev()
      .val(numProduct + 1);
  });

  /*==================================================================
    [ Rating ]*/
  $(".wrap-rating").each(function () {
    var item = $(this).find(".item-rating");
    var rated = -1;
    var input = $(this).find("input");
    $(input).val(0);

    $(item).on("mouseenter", function () {
      var index = item.index(this);
      var i = 0;
      for (i = 0; i <= index; i++) {
        $(item[i]).removeClass("zmdi-star-outline");
        $(item[i]).addClass("zmdi-star");
      }

      for (var j = i; j < item.length; j++) {
        $(item[j]).addClass("zmdi-star-outline");
        $(item[j]).removeClass("zmdi-star");
      }
    });

    $(item).on("click", function () {
      var index = item.index(this);
      rated = index;
      $(input).val(index + 1);
    });

    $(this).on("mouseleave", function () {
      var i = 0;
      for (i = 0; i <= rated; i++) {
        $(item[i]).removeClass("zmdi-star-outline");
        $(item[i]).addClass("zmdi-star");
      }

      for (var j = i; j < item.length; j++) {
        $(item[j]).addClass("zmdi-star-outline");
        $(item[j]).removeClass("zmdi-star");
      }
    });
  });

  /*==================================================================
    [ Show modal1 ]*/
  $(".js-show-modal1").on("click", function (e) {
    e.preventDefault();
    $(".js-modal1").addClass("show-modal1");
  });

  $(".js-hide-modal1").on("click", function () {
    $(".js-modal1").removeClass("show-modal1");
  });
})(jQuery);

$(".js-select2").each(function () {
  $(this).select2({
    minimumResultsForSearch: 20,
    dropdownParent: $(this).next(".dropDownSelect2"),
  });
});

$(".js-pscroll").each(function () {
  $(this).css("position", "relative");
  $(this).css("overflow", "hidden");
  var ps = new PerfectScrollbar(this, {
    wheelSpeed: 1,
    scrollingThreshold: 1000,
    wheelPropagation: false,
  });

  $(window).on("resize", function () {
    ps.update();
  });
});

$(".gallery-lb").each(function () {
  $(this).magnificPopup({
    delegate: "a",
    type: "image",
    gallery: {
      enabled: true,
    },
    mainClass: "mfp-fade",
  });
});

$(document).ready(function () {
  // Xử lý sự kiện bấm vào icon Wishlist
  $(".js-addwish-b2, .js-addwish-detail").on("click", function (e) {
    e.preventDefault();
    var form = $(this).closest("form");
    var productId = form.find('input[name="productId"]').val();
    var nameProduct = form
      .closest(".block2, .p-r-50")
      .find(".js-name-b2, .js-name-detail")
      .text()
      .trim();

    $.ajax({
      url: form.attr("action"),
      type: "POST",
      data: { productId: productId },
      success: function (response) {
        // Cập nhật số lượng Wishlist trên navbar
        $.get("/Client/Wishlist/GetWishlistCount", function (count) {
          $(".icon-header-noti[data-notify]").each(function () {
            if ($(this).find(".zmdi-favorite-outline").length) {
              $(this).attr("data-notify", count);
            }
          });
        });

        // Hiển thị thông báo
        var isAdded = response.isAdded; // Giả định server trả về trạng thái
        var message = isAdded
          ? nameProduct + " is added to wishlist!"
          : nameProduct + " is removed from wishlist!";
        swal(message, "", "success");

        // Cập nhật trạng thái icon
        if (isAdded) {
          form
            .find(".btn-addwish-b2, .js-addwish-detail")
            .addClass("js-addedwish-b2 js-addedwish-detail");
        } else {
          form
            .find(".btn-addwish-b2, .js-addwish-detail")
            .removeClass("js-addedwish-b2 js-addedwish-detail");
        }
      },
      error: function (xhr) {
        var errorMessage = xhr.responseJSON?.message || "An error occurred.";
        swal("Error", errorMessage, "error");
      },
    });
  });
});

// Dark Mode Toggle
$(document).ready(function () {
  // Check for saved theme in localStorage
  const savedTheme = localStorage.getItem("theme") || "light";
  $("body").attr("data-theme", savedTheme);
  updateIcon(savedTheme);

  // Toggle theme on click
  $(".js-toggle-theme").click(function () {
    const currentTheme = $("body").attr("data-theme");
    const newTheme = currentTheme === "light" ? "dark" : "light";
    $("body").attr("data-theme", newTheme);
    localStorage.setItem("theme", newTheme);
    updateIcon(newTheme);
  });

  // Update icon based on theme
  function updateIcon(theme) {
    const icon = $(".js-toggle-theme i");
    if (theme === "dark") {
      icon.removeClass("zmdi-brightness-2").addClass("zmdi-brightness-7");
    } else {
      icon.removeClass("zmdi-brightness-7").addClass("zmdi-brightness-2");
    }
  }
});
