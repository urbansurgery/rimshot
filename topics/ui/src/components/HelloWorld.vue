<script>
  // this function will inject the link to CSS into iframe header
  function loadCSSToiFrame(iframeId, filename) {
    // var x = setInterval(function () {
    //   const iframe = document.getElementById(iframeId);
    //   let doc = iframe.contentDocument || iframe.contentWindow.document;

    //   doc.body.innerHTML += `<style>.v-application--wrap:first-child {  display: none !important; }</style>`;

    //   console.log(doc);
    // }, 1000);
  }

  // just call a function to load your CSS
  // this path should be relative your HTML location

  export default {
    name: 'HelloWorld',
    props: {
      msg: { type: String, default: 'Welcome to Your Vue.js App' },
    },
    computed: {
      stream() {
        return 'fd1d44405d';
      },
      commit() {
        return '1844b4c47c';
      },
      object() {
        return 'a9e42ded91a1733d7adf71fd17a28c56';
      }
      /* embed camera settings
       { x: cam[0], y: cam[1], z: cam[2] }, // position
          { x: cam[3], y: cam[4], z: cam[5] } // target
          */
    },
    mounted() {
      loadCSSToiFrame('speckle-viewer', 'stylesheet.css');
    },
  };
</script>

<template>
  <div class="speckle-viewer">
    <iframe
      id="speckle-viewer"
      class="responsive"
      :src="`https://speckle.xyz/embed?stream=${stream}&object=${object}&c=[43.4691,-99.46319,77.60994,-0.07739,-13.88552,1.8697,0,1]`"
    />
  </div>
</template>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped lang="scss">
  h3 {
    margin: 40px 0 0;
  }
  ul {
    list-style-type: none;
    padding: 0;
  }
  li {
    display: inline-block;
    margin: 0 10px;
  }
  a {
    color: #42b983;
  }
</style>

<style lang="scss">

  .speckle-viewer {
    position: relative;
    overflow: hidden;
    width: 100%;
    padding-top: 56.25%; /* 16:9 Aspect Ratio (divide 9 by 16 = 0.5625) */

  }
  .responsive {
    position: absolute;
    top: 2%;
    left: 2%;
    bottom: 2%;
    right: 2%;
    width: 96%;
    height: 96%;
    border: none;
  }
</style>
