namespace KFC.Model.Person {
    public class Person
    {
        public static int id_generator = 0;
        public int id;
        public (int, int) home;
        public (int, int) workplace;
        public float satisfaction_rate;
        public float satisfaction_target;
        public Person((int,int) home,(int,int) workplace, float satisfaction_rate, float satisfaction_target)
        {
            id = id_generator++;
            this.home = home;
            this.workplace = workplace;
            this.satisfaction_rate = satisfaction_rate;
            this.satisfaction_target = satisfaction_target;
        }
    }
}
