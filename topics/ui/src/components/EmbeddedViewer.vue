<script>
  export default {
    name: 'EmbeddedViewer',
    props: {}, // TODO: shift data from the route queries to the component props
    data() {
      return {
        input: {
          stream: this.$route.query.stream ?? 'fd1d44405d',
          object:
            this.$route.query.object ?? 'a9e42ded91a1733d7adf71fd17a28c56',
          commit: this.$route.query.commit ?? '1844b4c47c',
          overlay: this.$route.query.overlay,
          camera:
            this.$route.query.c ??
            '[43.4691,-99.46319,77.60994,-0.07739,-13.88552,1.8697,0,1]',
          filter: this.$route.query.filter,
        },
      };
    },
  };
</script>

<template>
  <div class="speckle-viewer">
    <iframe
      id="speckle-viewer"
      class="responsive"
      :src="`https://speckle.xyz/embed?stream=${input.stream}&object=${input.object}&c=${input.c}&overlay=${input.overlay}&filter=${input.filter}`"
    />
  </div>
</template>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped lang="scss">
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
