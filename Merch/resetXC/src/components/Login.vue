<template>
  <div class="login">
    <el-row :gutter="20">
      <el-col :span="6" :offset="12">
        <el-form ref="form" label-width="80px">
          <el-form-item label="用户名">
            <el-input v-model="name"></el-input>
          </el-form-item>
          <el-form-item label="密码">
            <el-input v-model="password"></el-input>
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="onSubmit">登录</el-button>
          </el-form-item>
        </el-form>
      </el-col>
    </el-row>
  </div>
</template>

<script>

  export default {
    name: 'Login',
    data() {
      return {
        name: '',
        password: '',
      }
    },
    methods: {
      // onSubmit() {
      // if(this.name=='lyong'&&this.password=='123456'){
      //   this.$router.replace('/Home')
      // }
      //   this.$axios.get('api/user', {
      //     userName: this.name,
      //     password: this.password
      //   })
      //     .then(function (response) {
      //       console.log(response);
      //     })
      //     .catch(function (error) {
      //       console.log(error);
      //     });
      // }
      onSubmit() {
        this.$axios.post('http://192.168.1.73:8080/XCCloud/Login?action=CheckUser', {
          userName: this.name,
          password: this.password
        }, {
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
          }
        })
          .then(function (response) {
            console.log(response);
          })
          .catch(function (error) {
            console.log(error);
          });
      }
    }
  }
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
  h1, h2 {
    font-weight: normal;
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
