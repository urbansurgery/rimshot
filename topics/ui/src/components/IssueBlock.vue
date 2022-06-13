<script>
  import Vue, { PropType } from 'vue';
  import { db } from '@/plugins/firebase';
  import { mapActions, mapState, Store } from 'vuex';

  const IssueType = {
    CLASH: 'Model Clash',
    DATA_QUALITY: 'Data Quality',
    DESIGN: 'Design Issue',
    EXCLUSION: 'Exclusion Zone Breach',
    MODEL_ALIGNMENT: 'Model Alignment',
    PLACEHOLDER: 'Placeholder Alignment',
    SCOPE_GAP: 'Scope Gap',
    IMPLEMENTATION: 'Scope Implementation',
    PROVISION: 'Spatial Provision',
    SPATIAL_TYPE: 'Spatial Type',
  };

  export default Vue.extend({
    name: 'IssueBlock',
    props: {
      issue: {
        type: Object,
        default: () => {},
        required: true,
      },
      embedded: {
        type: Boolean,
        default: false,
      },
      views: {
        type: Array,
        default: () => [],
        required: true,
      },
      showDeletedViews: {
        type: Boolean,
        default: false,
      },
      analystsIssue: {
        type: Boolean,
        default: false,
      },
      issueCount: {
        type: Number,
        default: 1,
      },
      id: {
        type: String,
        required: true,
      },
      isSelected: {
        type: Boolean,
        default: false,
      },
      number: {
        type: Number,
        default: 1,
      },
      index: {
        type: Number,
        default: 0,
      },
      summary: {
        type: String,
        default: '',
      },
      listPosition: {
        type: String,
        default: undefined,
      },
      description: {
        type: String,
        default: '',
      },
      actionRequired: {
        type: String,
        default: '',
      },
      priority: {
        type: String,
        default: undefined,
      },
      type: {
        type: String,
        default: undefined,
      },
      elements: {
        type: Array,
        default: () => {
          return [];
        },
      },
      elementLabels: {
        type: Array,
        default: () => {
          return [];
        },
      },
      actioners: {
        type: Array,
        default: () => {
          return [];
        },
      },
      loadingImages: {
        type: Boolean,
        default: false,
      },
    },
    data: () => {
      return {
        priorities: ['Critical', 'High', 'Low'],
        types: [
          { header: 'Geometry' },
          { text: 'Design Issue', value: 'Design Issue' },
          { text: 'Model Clash', value: 'Model Clash' },
          { text: 'Placeholder Alignment', value: 'Placeholder Alignment' },
          { text: IssueType.MODEL_ALIGNMENT, value: IssueType.MODEL_ALIGNMENT },
          { header: 'Spatial' },
          { text: IssueType.EXCLUSION, value: IssueType.EXCLUSION },
          { text: IssueType.PROVISION, value: IssueType.PROVISION },
          { header: 'Scope' },
          { text: IssueType.SCOPE_GAP, value: IssueType.SCOPE_GAP },
          { text: IssueType.IMPLEMENTATION, value: IssueType.IMPLEMENTATION },
          { text: IssueType.SPATIAL_TYPE, value: IssueType.SPATIAL_TYPE },
          { header: 'Data Quality' },
          { text: IssueType.DATA_QUALITY, value: IssueType.DATA_QUALITY },
        ],
      };
    },
    computed: {
      ...mapState({
        commitProgress: (state) => state.commitProgress,
        commitElements: (state) => state.commitElements,
        commitNested: (state) => state.commitNested,
        commitGeometry: (state) => state.commitGeometry,
      }),
      comments() {
        const mockComments = [
          'This is fantastic',
          'This is not good',
          'This is a good one',
          'This is a bad one',
          'This is a terrible one',
          'This is a terrible one',
        ].slice(Math.floor(Math.random() * 2), 5);

        if (Math.random() > 0.5) {
          return mockComments;
        }

        return [mockComments[Math.floor(Math.random() * mockComments.length)]];
      },
      commentCount() {
        return this.comments.length;
      },
      host() {
        return this.$store?.state?.currentProject?.host;
      },
      isPrint() {
        return window.matchMedia('print').matches;
      },
      bindings() {
        return window.UIBindings;
      },
      speckleBranchName() {
        const branchName = `issues/${this.$store.state.currentProject?.key?.toLowerCase()}-${
          this.issue?.number
        }`;
        return this.issue?.speckle_branch || branchName;
      },
      speckleCommit() {
        const { speckle_stream, speckle_branch, speckle_host } = this.issue;
        let payload = {
          issueId: this.issue.id,
          stream: this.$store.state.currentProject?.speckle_stream,
          branch: this.speckleBranchName,
          host:
            speckle_host || this.host
              ? `https://${this.host}`
              : null || 'https://speckle.xyz',
        };
        if (payload?.stream) {
          return payload;
        }
        return null;
      },

      speckleView() {
        const {
          speckle_commit,
          speckle_stream,
          speckle_object,
          speckle_commit_object,
        } = this.issue;

        const speckle_host =
          this.issue?.speckle_host || this.host
            ? `https://${this.host}`
            : null || 'https://speckle.xyz';

        const urlParts = {
          host: speckle_host ?? 'https://speckle.xyz',
          stream: speckle_stream,
          commit: speckle_commit,
          commitObject: speckle_commit_object ?? null,
          object: speckle_object,
        };

        if (speckle_host && speckle_stream && speckle_commit_object) {
          return `${urlParts.host}/embed?stream=${urlParts.stream}&object=${urlParts.commitObject}`;
        }

        if (speckle_host && speckle_stream && speckle_object) {
          return `${urlParts.host}/embed?stream=${urlParts.stream}&object=${urlParts.object}`;
        }

        if (speckle_host && speckle_stream && speckle_commit) {
          return `${urlParts.host}/embed?stream=${urlParts.stream}&commit=${urlParts.commit}`;
        }

        return null;
      },
      speckleServer() {
        const {
          speckle_commit,
          speckle_stream,
          speckle_object,
          speckle_commit_object,
        } = this.issue;

        const speckle_host =
          this.issue?.speckle_host || this.host
            ? `https://${this.host}`
            : null || 'https://speckle.xyz';

        const urlParts = {
          host: speckle_host ?? 'https://speckle.xyz',
          stream: speckle_stream,
          commit: speckle_commit,
          commitObject: speckle_commit_object,
          object: speckle_object,
        };

        if (speckle_host && speckle_stream && speckle_commit) {
          return `${urlParts.host}/streams/${urlParts.stream}/commits/${urlParts.commit}`;
        }

        return null;
      },
    },
    watch: {},

    methods: {
      selectIssue() {
        this.$emit('select', { id: this.id, index: this.index });
      },
      deleteView(view) {
        //? Editing the views will cause the component to rerender.
        //? Should views be a direct binding to firestore or passed by Prop?
        //? Time will tell which is the lesser memory burden.
        if (view.id) {
          db.collection('views')
            .doc(view.id)
            .update({ deleted: !view.deleted });
        }
      },
      editIssueField(field, value) {
        this.$emit('edit', { id: this.id, field, value });
      },
      deleteIssue() {
        this.$emit('delete', this.id);
      },
      shiftIssue(direction) {
        if (
          (direction !== 'up' && direction !== 'down') ||
          (direction === 'up' && this.listPosition === 'First') ||
          (direction === 'down' && this.listPosition === 'Last')
        ) {
          return;
        }
        this.$emit('shift', { id: this.id, direction });
      },
      commitSelection() {
        if (this.bindings) {
          this.$store.commit('SET_COMMIT_PROGRESS', this.issue.id);
          this.bindings.commitSelection(this.speckleCommit);
        }
      },
    },
  });
</script>

<template>
  <v-row>
    <v-col
      cols="1"
      class="pa-0"
      style="width: 2rem; max-width: 2rem; cursor: pointer"
    >
      <v-sheet
        :color="isSelected ? 'primary' : 'background'"
        class="issue-selector pt-3"
        width="100%"
        height="100%"
        @click="selectIssue"
        ><div class="top-actions">
          <v-card-text class="issue-number pa-0">{{ number }}</v-card-text>
        </div>
        <!-- <div class="bottom-actions">
          <v-card-text class="shift-issue-up pa-0"
            ><v-btn icon @click.stop="shiftIssue('up')">
              <v-icon>mdi-arrow-up-bold</v-icon></v-btn
            ></v-card-text
          >
          <v-card-text class="shift-issue-down pa-0"
            ><v-btn icon @click.stop="shiftIssue('down')">
              <v-icon>mdi-arrow-down-bold</v-icon></v-btn
            ></v-card-text
          >
          <v-card-text class="delete-issue pa-0"
            ><v-btn icon @click.stop="deleteIssue">
              <v-icon>delete</v-icon></v-btn
            ></v-card-text
          >
        </div>-->
      </v-sheet>
    </v-col>
    <v-col cols="11" class="issue-row">
      <v-row class="pt-3 description-fields">
        <v-col class="">
          <v-text-field
            class="pt-0 mt-0 issue-field summary-field"
            :class="{ 'empty-field': !summary }"
            label="Summary"
            :value="summary"
            hint="Should succinctly describe what's being looked at."
            @change="editIssueField('summary', $event)"
          />
          <!-- <v-textarea
            class="issue-field description-field"
            :class="{ 'empty-field': !description }"
            name="input-7-1"
            label="Description"
            rows="2"
            :value="description"
            hint="Should contain description of the issue to be resolved."
            :auto-grow="true"
            @change="editIssueField('description', $event)"
          /> -->
          <v-textarea
            class="issue-field action-field description-field"
            :class="{ 'empty-field': !actionRequired }"
            name="input-7-1"
            label="Action to Resolve"
            :rows="2"
            :value="actionRequired"
            hint="Should contain the description of the specific action to resolve the issue."
            :auto-grow="true"
            @change="editIssueField('actionRequired', $event)"
          />
          <v-combobox
            :value="actioners"
            :items="actioners"
            label="Action Required From"
            :class="{
              'empty-field': !actioners || actioners.length < 1,
            }"
            multiple
            @change="editIssueField('actioners', $event)"
          />
        </v-col>
        <v-col cols="5" class="facts">
          <v-select
            class="issue-field priority-field"
            label="Priority"
            :items="priorities"
            clearable
            :value="priority"
            :class="{ 'empty-field': !priority }"
            :dense="!isPrint"
            @change="editIssueField('priority', $event)"
          />
          <v-select
            class="issue-field type-field"
            label="Type"
            :class="{ 'empty-field': !type }"
            :items="types"
            :value="type"
            :dense="!isPrint"
            clearable
            @change="editIssueField('type', $event)"
          />
          <v-combobox
            class="issue-field element-field"
            :class="{
              'empty-field': !elements || elements.length < 1,
            }"
            :value="elements"
            :items="elementLabels"
            label="Elements"
            multiple
            clearable
            small-chips
            :dense="!isPrint"
            @change="editIssueField('elements', $event)"
          />
        </v-col>
      </v-row>
      <v-row class="pb-4 ma-0">
        <v-col v-show="isSelected" cols="12" class="pa-0">
          <template v-if="views">
            <v-row class="issue-view-row" dense>
              <v-col v-for="(view, vindex) in views" :key="view.guid" cols="4">
                <v-hover v-slot="{ hover }">
                  <v-card>
                    <v-img
                      :src="`data:${view.format || 'image/png'};base64, ${
                        view.image
                      }`"
                      :class="{ deleted: view.deleted }"
                    >
                      <template #placeholder>
                        <v-row
                          class="fill-height ma-0"
                          align="center"
                          justify="center"
                        >
                          <v-progress-circular
                            indeterminate
                            color="grey lighten-5"
                          />
                        </v-row>
                      </template>
                    </v-img>
                    <v-btn
                      v-if="hover && embedded"
                      rounded
                      fab
                      x-small
                      class="ultra-small delete"
                      @click="deleteView(view)"
                    >
                      <v-icon>mdi-close-circle</v-icon>
                    </v-btn>
                    <v-btn
                      rounded
                      fab
                      x-small
                      class="ultra-small badge primary"
                    >
                      {{ vindex + 1 }}
                    </v-btn>
                  </v-card>
                </v-hover>
              </v-col>
            </v-row>
          </template>
        </v-col>
        <v-col cols="12" class="pa-0">
          <template v-if="views">
            <v-row class="issue-view-row-print" dense>
              <v-col v-for="view in views" :key="view.guid" cols="4">
                <v-hover v-slot="{ hover }">
                  <v-card>
                    <v-img
                      :src="`data:image/png;base64, ${view.image}`"
                      :lazy-src="`data:image/png;base64, ${view.thumbnail}`"
                      :class="{ deleted: view.deleted }"
                    >
                      <template #placeholder>
                        <v-row
                          class="fill-height ma-0"
                          align="center"
                          justify="center"
                        >
                          <v-progress-circular
                            indeterminate
                            color="grey lighten-5"
                          />
                        </v-row>
                      </template>
                    </v-img>
                    <v-btn
                      v-if="hover && isembedded"
                      rounded
                      fab
                      x-small
                      dark
                      class="ultra-small"
                      @click="deleteView(view)"
                    >
                      <v-icon>close</v-icon>
                    </v-btn>
                  </v-card>
                </v-hover>
              </v-col>
            </v-row>
          </template>
        </v-col>
        <v-col class="pa-0" cols="4">
          <span v-if="loadingImages" class="view-count">Loading Images…</span>
          <span v-else-if="views" class="view-count">{{
            `${views.filter((v) => !v.deleted).length} views`
          }}</span>
        </v-col>
      </v-row>
      <v-row v-show="isSelected" cols="12" class="embedded-view pa-0" dense
        ><v-col v-if="speckleView" cols="7" align-self="end" class="pr-3 pl-1">
          <iframe
            style="border: none"
            :src="speckleView"
            width="100%"
            height="250"
          />
        </v-col>
        <v-col cols="5" align-self="end">
          <div v-if="commitGeometry" style="min-height: 4px">
            <label>Geometry</label>
            <v-progress-linear :value="commitGeometry" />
          </div>
          <div v-if="commitNested" style="min-height: 4px">
            <label>Element Hierarchy</label>
            <v-progress-linear :value="commitNested" />
          </div>
          <div v-if="commitElements" style="min-height: 4px">
            <label>Elements</label>
            <v-progress-linear :value="commitElements" />
          </div>
          <!-- <span class="caption mb-4 pb-4"
                  >Commit will be made with<v-spacer /> objects selected in the
                  navigator</span
                ><v-spacer /> -->
          <v-btn
            v-if="bindings"
            color="info"
            class="mt-4"
            :disabled="Boolean(commitProgress)"
            @click="commitSelection"
          >
            Commit Selection
          </v-btn>
          <v-spacer />
          <v-btn
            v-if="speckleView"
            :href="speckleServer"
            target="_blank"
            color="primary"
            class="mt-5 mb-2"
          >
            Launch external viewer
          </v-btn>
        </v-col>
      </v-row>
      <v-spacer class="mb-7" />
      <v-row
        v-show="isSelected && commentCount"
        class="comments"
        light
        style="background-color: var(--v-primary-lighten4)"
      >
        <v-col cols="12" color="secondary">
          <v-textarea
            v-for="(comment, cindex) in comments"
            :key="cindex"
            class="issue-field action-field description-field white--text"
            :class="{ 'empty-field': !comment }"
            name="input-7-1"
            label="Comment"
            light
            :rows="2"
            :value="comment"
            hint="How can we improve this issue?"
            :auto-grow="true"
            @change="editIssueField('actionRequired', $event)"
        /></v-col>
      </v-row>
    </v-col>
  </v-row>
</template>

<style lang="scss" scoped>
  .row:hover .issue-selector:not(.selected) {
    background-color: var(--v-secondary-base) !important;
    color: white;
    opacity: 0.75;
  }
  .browser:not(.syncing) .current .issue-selector {
    position: relative;
    box-shadow: inset 2px 0 0 #999;
    color: #999;

    &::before {
      content: 'current';
      position: absolute;
      right: 50px;
      font-size: 16px;
      text-align: right;
      white-space: nowrap;
      top: 28px;
      color: #999;
    }
    &::after {
      content: '▶';
      position: absolute;
      right: 30px;
      font-size: 18px;
      text-align: right;
      white-space: nowrap;
      top: 28px;
      color: #999;
    }
    vertical-align: middle;

    background-color: #eaeaea !important;

    &.selected {
      background-color: var(--v-secondary-base) !important;
      color: #999;
    }
  }

  .browser.syncing .issue-selector.selected {
    background-color: var(--v-grey-darken1) !important;
  }

  .ultra-small {
    transform: scale(75%);
    position: absolute;
    &.delete {
      border: 2px white solid;
      right: -7.5px;
      top: -7.5px;
    }
    &.badge {
      top: 0;
      font-size: 1.24em;
    }
  }

  .deleted {
    opacity: 0.5;
    filter: saturate(0);
  }

  .view-count {
    font-size: 0.8rem;
    font-style: italic;
  }

  .issue-number,
  .delete-issue {
    text-align: center;
    font-weight: 500;
    font-size: 18px;
    line-height: 56px;
  }

  .selected .issue-number {
    color: white;
  }

  .browser:not(.syncing) .selected.current {
    .issue-selector {
      background-color: var(--v-grey-darken1) !important;
    }
  }

  .issue-field {
    &.priority-field {
      margin-top: 6px;
    }
  }
  .issue-block {
    box-shadow: inset 0 -1px 0 #99999933;
  }

  .empty {
    &.summary-field,
    &.action-field,
    &.priority-field,
    &.type-field {
      label {
        color: red;
      }
    }
  }

  .issue-block {
    .issue-selector {
      position: relative;

      .bottom-actions {
        position: absolute;
        bottom: 1ex;
        left: 0;
        right: 0;
        width: 100%;

        .v-card__text {
          display: none;

          &.delete-issue {
            color: black;
          }

          &:not(.delete-issue) .v-icon {
            color: white;
          }
        }
      }
    }

    &.selected .issue-selector:hover .v-card__text.delete-issue,
    .issue-selector:hover .v-card__text:not(.delete-issue) {
      display: block;
    }
  }

  .first .issue-selector .shift-issue-up .v-btn,
  .last .issue-selector .shift-issue-down .v-btn {
    display: none;
  }
</style>

<style lang="css">
  .v-progress-linear,
  .v-progress-linear__bar,
  .v-progress-linear__bar__determinate {
    transition: none !important;
  }
</style>

<style lang="scss">
  .element-field .v-chip--select.v-chip.v-size--small {
    border-radius: 6px;
    background-color: var(--v-grey-darken1);
    color: white;
    margin: 1px;
    &:last {
      margin-bottom: 3px;
    }
  }

  .element-field .v-select__selections {
    padding-bottom: 2px;
    padding-top: 2px;
  }

  .facts {
    .mdi-close {
      transform: scale(65%) translateX(1rem);
    }

    .v-select__selection {
      font-size: 0.85rem;
    }
  }
</style>

<style lang="scss" scoped id="print-settings">
  @media screen {
    .issue-view-row-print {
      display: none;
    }
  }

  @media print {
    .view-count,
    .empty-field,
    button,
    .v-input__append-inner,
    .description-fields .v-text-field__details {
      display: none !important;
    }

    .view-count,
    .empty-field,
    button,
    .v-input__append-inner,
    .description-fields .v-text-field__details {
      display: none !important;
      &::v-deep .view,
      &::v-deep .host,
      &::v-deep .in {
        display: none !important;
      }
    }
    .description-field {
      margin-top: 0;
    }

    .v-textarea textarea {
      height: auto !important;
      line-height: 15pt;
      font-size: 12pt;
      padding-top: 3pt;
    }

    .issue-block {
      break-inside: avoid;
      border: 0;
      box-shadow: none;
    }

    .browser:not(.syncing) .current div.issue-selector,
    .browser:not(.syncing) .row.selected.current div.issue-selector,
    .issue-selector,
    .row:hover div.issue-selector:not(.selected),
    .issue-selector.v-sheet,
    .issue-number {
      background: none !important;
      background-color: transparent !important;
      color: var(--v-default-base) !important;
      border: none !important;
      box-shadow: none !important;

      &::before,
      &::after {
        content: none !important;
      }
    }

    .row.issue-block {
      box-shadow: none;
      border-bottom: 1px solid var(--v-grey-base);
    }

    .v-input__slot::before {
      border: none !important;
    }
    .summary-field {
      font-weight: 500;
      label {
        display: none;
      }
    }

    .issue-field.priority-field {
      margin-top: -3px;
      font-weight: 500;
      padding-top: 3px;
      [role='button']::before {
        border: none;
      }
      label {
        display: none;
      }
      &::before {
        content: 'Priority';
        font-weight: 400;
        font-size: 0.8em;
        color: var(--v-grey-base);
        padding-top: 6px;
        padding-right: 1ex;
      }
    }

    .element-field .v-chip--select.v-chip.v-size--small {
      background-color: transparent;
      color: var(--v-default-base);
      border: 1px solid var(--v-grey-base);
      margin-top: 3px;
      padding: 0.5rem;
    }
  }
</style>
