<script>
  import Vue from 'vue';
  export default Vue.extend({
    name: 'BlankView',
  });
</script>

<template>
  <div id="blank-page">
    <v-container>
      <v-row justify="center" style=""
        ><v-col cols="9" align="center">
          <a href="./about"
            ><img
              src="../assets/logo.png"
              alt="Rimshot Logo"
              width="200"
              style="max-width: 100%"
          /></a>
        </v-col> </v-row
    ></v-container>
  </div>
</template>

<style lang="scss" scoped>
  #blank-page {
    height: 100%;
    font-size: 3rem;
    zoom: 2;
  }
</style>
