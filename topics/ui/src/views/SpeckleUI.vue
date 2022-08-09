<script>
  import Vue from 'vue';

  // for prototyping
  import {
    dummySelectionSets,
    dummyStreams,
    dummyAccounts,
    dummySenders,
    dummyBranches,
  } from '../mock-data/speckleui';

  export default {
    data: () => ({
      accounts: [],
      adHocSelection: 0,
      bindings: undefined,
      branches: ['main'],
      colorMode: false,
      commitMessage: '',
      coordinateRule: (v) => {
        if (!v.trim()) return true;
        if (!isNaN(parseFloat(v))) return true;
        return 'Coordinates must be a number';
      },
      coordinateSystem: 'model',
      defaultProperty: { category: '', property: '' },
      dialog: false,
      dialogDelete: false,
      editedIndex: -1,
      editedProperty: { category: '', property: '' },
      footerProps: {
        'disable-items-per-page': true,
        'items-per-page-options': [5],
      },
      headers: [
        { text: 'Category', value: 'category', sortable: false, width: '40%' },
        { text: 'Property', value: 'property', sortable: false, width: '40%' },
        { text: 'Actions', value: 'actions', sortable: false, width: '10%' },
      ],
      hiddenMode: false,
      isBound: false,
      lastSelectionMade: [],
      panel: null,
      propagateProperties: true,
      properties: [],
      savedSenders: [],
      selectionMode: false,
      selections: [],
      selectionSets: [],
      selectedBranch: 'main',
      selectedStream: null,
      selectedAccount: null,
      senders: [],
      showOptions: false,
      showProperties: false,
      streams: [],
      translationVectorZ: 0,
      translationVectorX: 0,
      translationVectorY: 0,
      units: 'm',
    }),
    computed: {
      hosted() {
        return this.isEmbedded ? 'Hosted in Navisworks' : 'Shown in Browser';
      },
      isEmbedded() {
        return this.$store.state.isEmbedded;
      },
      paneView() {
        return Boolean(window.UIBindings);
      },
      isSelectionMade() {
        return (
          (!this.selectionMode && this.adHocSelection > 0) ||
          (this.selectionMode && this.selections.length > 0)
        );
      },
      isTargetSet() {
        return (
          this.selectedAccount && this.selectedStream && this.selectedBranch
        );
      },
      canCommit() {
        return this.isSelectionMade && this.isTargetSet;
      },
      hasAccount() {
        return Boolean(this.selectedAccount);
      },
      hasStream() {
        return Boolean(this.selectedStream);
      },
      hasBranch() {
        return Boolean(this.selectedBranch);
      },
      settings() {
        return {
          colorMode: this.colorMode,
          coordinateSystem: this.coordinateSystem,
          hiddenMode: this.hiddenMode,
          selectionMode: this.selectionMode,
          selections: this.selections,
          target: {
            account: {
              host: this.selectedAccount?.host,
              email: this.selectedAccount?.email,
            },
            stream: {
              id: this.selectedStream?.id,
              name: this.selectedStream?.name,
            },
            branch: { name: this.selectedBranch?.name },
          },
          vector: {
            x: this.translationVectorX,
            y: this.translationVectorY,
            z: this.translationVectorZ,
          },
        };
      },
    },
    watch: {
      dialog(val) {
        val || this.close();
      },
      dialogDelete(val) {
        val || this.closeDelete();
      },
    },
    async mounted() {
      this.Bindings = window.UIBindings;

      if (typeof this.Bindings === 'undefined' || this.Bindings === null) {
        this.showNotEmbeddedError = true;
        this.selectionSets = dummySelectionSets;
        this.accounts = dummyAccounts;
        this.selectedAccount = this.accounts[0];
        this.streams = dummyStreams;
        this.selectedStream = this.streams[0];
        this.branches = dummyBranches;
        this.selectedBranch = this.branches[0];
        this.senders = dummySenders.map((sender) => {
          return { ...sender, show: false };
        });
        this.adHocSelection = 1;
        return;
      }

      this.isBound = true;

      this.getNewSelectionSets();
      this.getSavedSenders();

      this.accounts = dummyAccounts;
      this.selectedAccount = this.accounts[0];
      this.streams = dummyStreams;
      this.selectedStream = this.streams[0];
      this.branches = dummyBranches;
      this.selectedBranch = this.branches[0];
      this.adHocSelection = 1;
    },
    created() {
      window.EventBus = new Vue();

      window.EventBus.$on('changed-selection-sets', (payload) => {
        console.log('%cSpeckleRoamer: Go get new sets', 'color: hotpink');
        this.getNewSelectionSets();
      });

      this.initialiseProperties();
    },
    methods: {
      initialiseProperties() {
        this.properties = [
          {
            category: 'Item',
            property: 'Name',
          },
          {
            category: 'Material',
            property: 'Name',
          },
          {
            category: 'Material',
            property: 'Color',
          },
        ];
      },
      refresh() {
        this.Bindings?.refresh && this.Bindings.refresh();
      },
      showDevTools() {
        this.Bindings?.showDev && this.Bindings.showDev();
      },
      darkMode() {
        this.$vuetify.theme.dark = !this.$vuetify.theme.dark;
      },
      async getAccounts() {
        if (!this.isBound) {
          this.accounts = dummyAccounts;
          this.selectedAccount = dummyAccounts[0];
          return;
        }
        return await this.Bindings.getAccounts();
      },
      async getStreams() {
        if (!this.isBound || !this.hasAccount) {
          s;
          this.streams = dummyStreams;
          this.selectedStream = null;
          return;
        }
        return await this.Bindings.getStreams();
      },
      async getBranches() {
        if (!this.isBound || !this.hasStream || !this.hasAccount) {
          this.branches = ['main'];
          this.selectedBranch = 'main';
          return;
        }
        return await this.Bindings.getBranches();
      },
      async getNewSelectionSets() {
        if (!this.isBound) {
          this.selectionSets = dummySelectionSets;
          return;
        }
        this.selectionSets = await this.Bindings.getSets();
      },
      changeSelectionMode() {
        if (!this.selectionMode) {
          this.selections = [];
        }
      },
      changeAccount() {
        this.selectedStream = null;
        this.selectedBranch = null;
        this.getStreams();
      },
      changeStream() {
        this.branches = ['main'];
        this.selectedBranch = 'main';
        this.getBranches();
      },

      options(settings) {
        let defaultSettings = false;
        if (!settings) {
          defaultSettings = true;
          settings = this.settings;
        }
        const options = [];
        if (
          defaultSettings &&
          settings.target.account &&
          settings.target.account.host
        ) {
          options.push({
            type: 'account',
            labelText: `host: ${settings.target.account.host}`,
          });
          options.push({
            type: 'account',
            labelText: `account: ${settings.target.account.email}`,
          });
        }
        if (
          defaultSettings &&
          settings.target.stream &&
          settings.target.stream.name
        ) {
          options.push({
            type: 'target',
            labelText: `stream: ${settings.target.stream.name}`,
          });
        }
        if (
          defaultSettings &&
          settings.target?.branch &&
          settings.target.branch.name
        ) {
          options.push({
            type: 'target',
            labelText: `branch: ${settings.target.branch.name}`,
          });
        }
        if (settings.selectionMode && settings.selections?.length > 0) {
          settings.selections
            .map((s) => ({
              type: 'selection',
              labelText: `selection: ${s.Name}`,
            }))
            .forEach((s) => options.push(s));
        }
        if (!settings.selectionMode && this.adHocSelection > 0) {
          options.push({
            type: 'selection',
            labelText: `selection: current`,
          });
        }
        if (!settings.colorMode) {
          options.push({
            type: 'option',
            labelText: `colors: permanent`,
          });
        } else {
          options.push({
            type: 'option',
            labelText: `colors: override`,
          });
        }

        if (!settings.hiddenMode) {
          options.push({
            type: 'option',
            labelText: `hidden: include`,
          });
        } else {
          options.push({
            type: 'option',
            labelText: `hidden: exclude`,
          });
        }
        if (settings.coordinateSystem) {
          options.push({
            type: 'option',
            labelText: `coordinates: ${settings.coordinateSystem}`,
          });
          if (settings.coordinateSystem === 'vector' && settings.vector) {
            options.push({
              type: 'option',
              labelText: `vector: ${settings.vector.x || 0}, ${
                settings.vector.y || 0
              }, ${settings.vector.z || 0}`,
            });
          }
        }

        return options;
      },
      inverseShow(show) {
        console.log(show);
        if (!show) {
          return true;
        }
        return false;
      },
      deleteSender(senderIndex) {
        this.senders = this.senders.filter((s, i) => i !== senderIndex);
      },
      saveSender() {
        this.senders.push({ ...this.settings, show: true });
      },

      refreshAccounts() {},
      editProperty(property) {
        this.editedIndex = this.properties.indexOf(property);
        this.editedProperty = Object.assign({}, property);
        this.dialog = true;
      },
      deleteProperty(property) {
        this.editedIndex = this.properties.indexOf(property);
        this.editedProperty = Object.assign({}, property);
        this.dialogDelete = true;
      },
      deletePropertyConfirm() {
        this.properties.splice(this.editedIndex, 1);
        this.closeDelete();
      },
      close() {
        this.dialog = false;
        this.$nextTick(() => {
          this.editedProperty = Object.assign({}, this.defaultProperty);
          this.editedIndex = -1;
        });
      },
      closeDelete() {
        this.dialogDelete = false;
        this.$nextTick(() => {
          this.editedProperty = Object.assign({}, this.defaultProperty);
          this.editedIndex = -1;
        });
      },
      save() {
        if (this.editedIndex > -1) {
          Object.assign(this.properties[this.editedIndex], this.editedProperty);
        } else {
          this.properties.push(this.editedProperty);
        }
        this.close();
      },
      async getSavedSenders() {
        console.debug(
          `This calls to the host Roamer instance to check the active documents` +
            `data source for serialized senders.`
        );
        if (!isBound) {
          this.senders = dummySenders.map((sender) => {
            return { ...sender, show: true };
          });
          return;
        }
        this.senders = await this.Bindings.getSenders();
      },
      async commit(options) {
        if (!this.isBound) {
          return;
        }

        if (!this.canCommit) {
          return;
        }

        if (this.Bindings?.speckleCommit) {
          return;
        }

        await this.Bindings.speckleCommit(options);
      },
      commitSender(sender) {
        console.log(sender);
      },
    },
  };
</script>

<template>
  <div id="speckleui" :class="{ hosted: paneView }">
    <v-container class="px-5" fluid
      ><v-row align="stretch"
        ><v-col
          v-for="(sender, i) in senders"
          :key="i"
          cols="6"
          class="d-flex flex-column"
          ><v-card
            elevation="4"
            outlined
            class="d-flex flex-column flex-grow-1 rounded-lg"
            ><v-card-text class="pb-0 flex-grow-1"
              ><span class="caption">
                [{{ sender.target.account.host }} ] -
                {{ sender.target.account.email }} </span
              ><v-card-subtitle class="pa-0 pt-2 font-weight-bold"
                ><span class="text-truncate d-inline-block">{{
                  sender.target.stream.name
                }}</span
                ><span class="text-truncate d-inline-block"
                  ><v-icon small>mdi-source-branch</v-icon
                  >{{ sender.target.branch.name }}</span
                ></v-card-subtitle
              > </v-card-text
            ><v-card-actions class="mt-0 pt-0">
              <v-col class="ma-0 pa-0 d-flex justify-end">
                <v-btn
                  fab
                  x-small
                  icon
                  class="mr-auto"
                  @click="commitSender(sender)"
                >
                  <v-icon color="primary">mdi-send</v-icon>
                </v-btn>
                <v-btn icon fab x-small text @click="loadSender(i)">
                  <v-icon>mdi-pencil</v-icon>
                </v-btn>
                <v-btn icon fab x-small text @click="deleteSender(i)">
                  <v-icon>mdi-delete</v-icon>
                </v-btn>
                <v-btn icon @click="sender.show = inverseShow(sender.show)">
                  <v-icon>{{
                    sender.show ? 'mdi-chevron-up' : 'mdi-chevron-down'
                  }}</v-icon>
                </v-btn>
              </v-col></v-card-actions
            >
            <v-expand-transition>
              <div v-show="sender.show" class="px-4 pb-4">
                <v-chip
                  v-for="(option, j) in options(sender)"
                  :key="j"
                  small
                  :class="option.type"
                  class="black--text mt-2 mr-2"
                  >{{ option.labelText }}</v-chip
                >
              </div>
            </v-expand-transition>
          </v-card>
        </v-col>
      </v-row>
      <v-divider class="mt-5"></v-divider>
      <!-- <fieldset class="mt-5"> -->
      <!-- <legend>Commit</legend> -->
      <v-text-field
        v-model="commitMessage"
        outlined
        class="mx-0 mt-5"
        label="Commit Message"
        counter="100"
        single-line
        persistent-hint
        hint="Defaults to number of elements, your selection type and the coordinate system."
      >
      </v-text-field>
      <v-row class="options px-0 mx-0 py-3">
        <v-chip
          v-for="(option, i) in options()"
          :key="i"
          :class="option.type"
          class="black--text mt-2 mr-2"
          >{{ option.labelText }}</v-chip
        >
      </v-row>
      <!-- </fieldset> -->
      <v-row class="d-flex pa-0 mt-2 pt-5 pb-2 justify">
        <v-col>
          <v-btn
            text
            @click="(showOptions = !showOptions) && (showProperties = false)"
          >
            <v-icon
              >{{ showOptions ? 'mdi-chevron-up' : 'mdi-chevron-down' }}
            </v-icon>
            Options</v-btn
          >
          <v-btn
            class="mr-auto"
            text
            @click="(showProperties = !showProperties) && (showOptions = false)"
          >
            <v-icon
              >{{ showProperties ? 'mdi-chevron-up' : 'mdi-chevron-down' }}
            </v-icon>
            Properties</v-btn
          ></v-col
        >
        <v-col class="text-right">
          <span v-if="!isSelectionMade" class="warning--text">
            No Objects selected.</span
          >
          <v-btn
            class="mb-1"
            :disabled="!canCommit || !selectionMode"
            dense
            @click="saveSender"
            >Save Sender</v-btn
          >
          <v-btn
            class="mb-1 ml-5"
            color="primary"
            dense
            :disabled="!canCommit"
            @click="commit"
          >
            Quick Commit
          </v-btn></v-col
        ></v-row
      >

      <v-expand-transition
        ><div v-show="showProperties" class="properties-table">
          <v-data-table
            dense
            :mobile-breakpoint="0"
            fixed-header
            :items="properties"
            :headers="headers"
            sort-by="['category','property']"
            :footer-props="footerProps"
          >
            <template #[`item.category`]="{ item }">
              {{ item.category }}
              <span
                v-if="item.category === 'Item' && propagateProperties"
                class="ml-1 caption warning--text"
                >(Not propagated)</span
              >
            </template>
            <template #[`item.property`]="{ item }">
              {{ item.property }}
              <span
                v-if="item.category === 'Item' && propagateProperties"
                class="ml-1 caption warning--text"
                >(Not propagated)</span
              >
            </template>
            <template #top>
              <!-- <v-toolbar flat> -->
              <v-row>
                <v-col class="pl-5">
                  <v-dialog v-model="dialog" max-width="500px" persistent>
                    <template #activator="{ on, attrs }">
                      <v-btn
                        class="mb-2"
                        v-bind="attrs"
                        small
                        dense
                        text
                        color="primary"
                        v-on="on"
                      >
                        Add Property
                      </v-btn>
                    </template>
                    <v-card>
                      <v-card-text>
                        <v-container>
                          <v-row>
                            <v-col cols="12" sm="6" md="4">
                              <v-text-field
                                v-model="editedProperty.category"
                                label="Property Category"
                              ></v-text-field>
                            </v-col>
                            <v-col cols="12" sm="6" md="4">
                              <v-text-field
                                v-model="editedProperty.property"
                                label="Property Name"
                              ></v-text-field>
                            </v-col>
                          </v-row>
                        </v-container>
                      </v-card-text>

                      <v-card-actions>
                        <v-spacer></v-spacer>
                        <v-btn text @click="close"> Cancel </v-btn>
                        <v-btn text @click="save"> Save </v-btn>
                      </v-card-actions>
                    </v-card>
                  </v-dialog>
                  <v-dialog
                    v-model="dialogDelete"
                    max-width="500px"
                    persistents
                    content-class="my-custom-dialog"
                  >
                    <v-card>
                      <v-card-title class="text-h5"
                        >Are you sure you want to delete this
                        property?</v-card-title
                      >
                      <v-card-actions>
                        <v-spacer></v-spacer>
                        <v-btn text @click="closeDelete">Cancel</v-btn>
                        <v-btn text @click="deletePropertyConfirm">OK</v-btn>
                        <v-spacer></v-spacer>
                      </v-card-actions>
                    </v-card>
                  </v-dialog> </v-col
                ><v-col>
                  <v-switch
                    v-model="propagateProperties"
                    class="mt-0"
                    small
                    dense
                    :label="`${
                      propagateProperties
                        ? 'Properties Propagated'
                        : 'Properties per Node'
                    }`"
                  >
                    <!-- </v-toolbar> -->
                  </v-switch>
                </v-col></v-row
              ></template
            >
            <template #[`item.actions`]="{ item }">
              <v-icon small class="mr-2" @click="editProperty(item)">
                mdi-pencil
              </v-icon>
              <v-icon small @click="deleteProperty(item)"> mdi-delete </v-icon>
            </template>
          </v-data-table>
        </div></v-expand-transition
      >
      <v-expand-transition>
        <div v-show="showOptions" class="px-4 pb-2 pt-2 options-panel">
          <v-form>
            <fieldset class="mb-5 pt-2">
              <legend>Target</legend>
              <v-select
                v-model="selectedAccount"
                :items="accounts"
                label="Account"
                single-line
                prepend-icon="mdi-account-circle"
                dense
                item-text="displayname"
                return-object
                no-data-text="No accounts found. Check in SpeckleManager."
              >
              </v-select>
              <v-select
                v-model="selectedStream"
                dense
                :disabled="!hasAccount"
                single-line
                return-object
                prepend-icon="mdi-view-stream"
                item-text="name"
                :items="streams"
                label="Stream"
                no-data-text="No streams found. Check account used has write access to a stream."
              >
              </v-select>
              <v-select
                v-model="selectedBranch"
                :disabled="!hasStream || !hasAccount"
                dense
                return-object
                item-text="name"
                :items="branches"
                prepend-icon="mdi-source-branch"
                label="Branch"
                single-line
              >
              </v-select>
              <v-col class="text-right">
                <v-btn v-if="isEmbedded" x-small text @click="refreshAccounts">
                  Refresh
                </v-btn>
              </v-col>
            </fieldset>

            <v-row class="px-5">
              <v-col class="pa-0">
                <v-switch
                  v-model="colorMode"
                  inset
                  dense
                  :label="`${
                    !colorMode ? 'Permanent Colours' : 'Override Colours'
                  }`"
                >
                </v-switch>
              </v-col>
              <v-col class="pa-0">
                <v-switch
                  v-model="hiddenMode"
                  inset
                  disabled
                  dense
                  :label="`${
                    hiddenMode
                      ? 'Omit Hidden Elements'
                      : 'Include Hidden Elements'
                  }`"
                >
                </v-switch>
              </v-col>
            </v-row>
            <v-row class="px-5 mt-0 mb-2">
              <v-col class="pa-0">
                <v-switch
                  v-model="selectionMode"
                  inset
                  dense
                  :label="`${
                    !selectionMode
                      ? 'Ad Hoc (Current Selection)'
                      : 'Saved Selection/Search'
                  }`"
                  @change="changeSelectionMode"
                >
                </v-switch>
              </v-col>
            </v-row>
            <fieldset
              id="selections"
              :class="{ inactive: !selectionMode }"
              :disabled="!selectionMode"
            >
              <legend>Selections</legend>
              <v-treeview
                v-model="selections"
                :disabled="!selectionMode"
                selectable
                :multiple-selected="false"
                selected-color="primary"
                :items="selectionSets"
                selection-type="leaf"
                item-key="Guid"
                item-children="Children"
                :open-on-click="selectionMode"
                item-text="Name"
                dense
                :return-object="true"
              >
                <template #label="{ item }">
                  {{ item.Name }}
                  <span v-if="item.Type === 'search'" class="selection-type">
                    (search)
                  </span>
                  <span v-if="item.Type === 'selection'" class="selection-type">
                    (selection)
                  </span>
                </template>
              </v-treeview>
            </fieldset>

            <!-- <fieldset class="mt-5" disabled>
                <legend>Model Layers</legend>
              </fieldset> -->
            <fieldset class="mt-5">
              <legend>Coordinate System</legend>
              <v-radio-group v-model="coordinateSystem" row>
                <v-radio value="model" label="Model Coordinates"></v-radio>
                <v-radio value="vector" label="Translation Vector"></v-radio>
                <v-radio value="center" label="Center to 0,0"> </v-radio
              ></v-radio-group>
              <v-row>
                <v-col>
                  <v-text-field
                    v-model="translationVectorX"
                    :disabled="coordinateSystem != 'vector'"
                    dense
                    label="0.000"
                    outlined
                    single-line
                    prefix="X"
                    :suffix="units"
                    hint="X coordinate of committed model origin"
                  >
                  </v-text-field>
                </v-col>
                <v-col>
                  <v-text-field
                    v-model="translationVectorY"
                    :disabled="coordinateSystem != 'vector'"
                    outlined
                    single-line
                    label="0.000"
                    dense
                    prefix="Y"
                    :suffix="units"
                    hint="Y coordinate of committed model origin"
                  >
                  </v-text-field>
                </v-col>
                <v-col>
                  <v-text-field
                    v-model="translationVectorZ"
                    disabled
                    outlined
                    single-line
                    label="0.000"
                    dense
                    prefix="Z"
                    :suffix="units"
                    hint="Z coordinate of committed model origin"
                  >
                  </v-text-field>
                </v-col>
              </v-row>
            </fieldset>
            <v-col class="text-right ma-0 pa-0 pt-5">
              <v-btn
                class="ma-0"
                :disabled="!canCommit || !selectionMode"
                dense
                @click="saveSender"
                >Save Sender</v-btn
              ></v-col
            >
          </v-form>
        </div>
      </v-expand-transition>
    </v-container>
    <v-footer fixed>
      <v-row>
        <v-col>
          <v-btn
            v-if="isEmbedded"
            :disabled="!isBound"
            x-small
            text
            @click="showDevTools"
            >Devtools</v-btn
          >
        </v-col>
        <v-col>
          <v-btn x-small text @click="darkMode">
            {{ $vuetify.theme.dark ? 'Light' : 'Dark' }}
          </v-btn>
        </v-col>
      </v-row>
    </v-footer>
  </div>
</template>

<style lang="scss" scoped>
  #speckleui.hosted {
    zoom: 70%;
  }

  .options-panel {
    margin-bottom: 50px;
  }

  fieldset {
    padding: 0 1rem 1ex;

    legend {
      color: inherit;
      padding: 0 1ex;
    }

    border: 1px solid #666;
    border-radius: 0.5rem;

    &.inactive legend,
    &[disabled] legend {
      color: #666;
    }

    &#selections .v-treeview {
      max-height: 10rem;
      overflow-y: auto;

      &::v-deep .v-treeview {
        zoom: 70%;
      }
    }
  }

  .selection-type {
    color: #999;
    font-style: italic;
  }

  .options,
  .v-card {
    .v-chip {
      &.option {
        background-color: #b5b5b5;
      }
      &.account {
        background-color: #c0c0a1;
      }
      &.selection {
        background-color: #98bcbc;
      }
      &.target {
        background-color: #c5aac5;
      }
    }
  }
</style>

<style scoped>
  ::v-deep .v-data-footer {
    justify-content: end;
  }
  ::v-deep .properties-table .v-data-table {
    background-color: transparent;
  }
  .v-dialog .v-card__title {
    -webkit-hyphens: none;
    -moz-hyphens: none;
    hyphens: none;
  }
</style>
