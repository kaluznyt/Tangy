var gulp = require('gulp');
var uglify = require('gulp-uglify');
var concat = require('gulp-concat');
var rimraf = require("rimraf");
var merge = require('merge-stream'); 

gulp.task("clean", function (cb) {
    return rimraf("wwwroot/lib/", cb);
});

// Dependency Dirs
var deps = {
    "jquery": {
        "jquery.*": "dist"
    },
    "jquery-validation": {
        "dist/**": "dist"
    },
    "jquery-validation-unobtrusive": {
        "dist/**": ""
    },
    "bootstrap": {
        "dist/**": "dist"
    },
};

//gulp.task("minify", function () {

//    var streams = [
//        gulp.src(["wwwroot/js/*.js", '!wwwroot/js/tour*.js', '!wwwroot/js/contact.js'])
//        .pipe(uglify())
//        .pipe(concat("wilderblog.min.js"))
//        .pipe(gulp.dest("wwwroot/lib/site")),
//        gulp.src(["wwwroot/js/contact.js"])
//        .pipe(uglify())
//        .pipe(concat("contact.min.js"))
//        .pipe(gulp.dest("wwwroot/lib/site"))
//    ];

//    return merge(streams);
//});

gulp.task("scripts", function () {

    var streams = [];

    for (var prop in deps) {
        console.log("Prepping Scripts for: " + prop);

        for (var itemProp in deps[prop]) {
            console.log("node_modules/" + prop + "/" + itemProp);
            streams.push(gulp.src("node_modules/" + prop + "/" + itemProp)
                .pipe(gulp.dest("wwwroot/lib/" + prop + "/" + deps[prop][itemProp])));
        }
    }

    return merge(streams);

});



gulp.task("minify", function () {

    var streams = [
        gulp.src(["wwwroot/js/*.js", '!wwwroot/js/tour*.js', '!wwwroot/js/contact.js'])
        .pipe(uglify())
        .pipe(concat("wilderblog.min.js"))
        .pipe(gulp.dest("wwwroot/lib/site")),
        gulp.src(["wwwroot/js/contact.js"])
        .pipe(uglify())
        .pipe(concat("contact.min.js"))
        .pipe(gulp.dest("wwwroot/lib/site"))
    ];

    return merge(streams);
});


gulp.task("default", gulp.series('clean', 'scripts'));