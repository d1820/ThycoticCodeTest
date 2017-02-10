using System;
using NUnit.Framework;

namespace CodeTest
{
    class Program
    {
        static void Main(string[] args)
        {

        }
    }



    public class Car
    {
        public decimal PurchaseValue { get; set; }
        public int AgeInMonths { get; set; }
        public int NumberOfMiles { get; set; }
        public int NumberOfPreviousOwners { get; set; }
        public int NumberOfCollisions { get; set; }
    }

    public static class CarExtensions
    {
        public static decimal GetWorth(this Car car)
        {
            var adjustAgeDecorator = new AdjustAgeDecorator();
            var adjustMilesDecorator = new AdjustMilesDecorator();
            var adjustPreviousOwnerDecorator = new AdjustPreviousOwnerDecorator();
            var adjustCollisionDecorator = new AdjustCollisionDecorator();

            adjustAgeDecorator.SetNextAdjustment(adjustMilesDecorator);
            adjustMilesDecorator.SetNextAdjustment(adjustPreviousOwnerDecorator);
            adjustPreviousOwnerDecorator.SetNextAdjustment(adjustCollisionDecorator);
            
            var result = adjustAgeDecorator.Calculate(car, car.PurchaseValue);
            return result;
        }
    }

    public class PriceDeterminator
    {
        public decimal DetermineCarPrice(Car car)
        {
            return car.GetWorth();
        }
    }

    public abstract class Component
    {
        public abstract decimal Calculate(Car car, decimal value);
    }
    public abstract class Decorator : Component
    {
        protected Component Component;

        public void SetNextAdjustment(Component nextComponent)
        {
            Component = nextComponent;
        }
    }

    public class AdjustAgeDecorator : Decorator
    {
        private const decimal DISCOUNT = .5M;
        private const int MONTH_MAX = 120;

        public override decimal Calculate(Car car, decimal value)
        {
            var discount1 = Convert.ToDecimal(Math.Min(car.AgeInMonths, MONTH_MAX) * DISCOUNT / 100);
            var adjustedAmount = value - (value * discount1);
            if (Component != null)
            {
                return Component.Calculate(car, adjustedAmount);
            }
            return adjustedAmount;
        }

    }

    public class AdjustMilesDecorator : Decorator
    {
        private const decimal DISCOUNT = .2M;
        private const int MILEAGE_MAX = 150000;
        private const decimal MILEAGE_INC = 1000M;
        public override decimal Calculate(Car car, decimal value)
        {
            var discount1 = Convert.ToDecimal( Math.Truncate(Math.Min(car.NumberOfMiles, MILEAGE_MAX) / MILEAGE_INC) * DISCOUNT / 100);
            var adjustedAmount = value - (value * discount1);
            if (Component != null)
            {
                return Component.Calculate(car, adjustedAmount);
            }
            return adjustedAmount;
        }
    }

    public class AdjustPreviousOwnerDecorator : Decorator
    {
        private const decimal DISCOUNT = .25M;
        private const decimal BONUS = 1.10M;
        public override decimal Calculate(Car car, decimal value)
        {
            var adjustedAmount = value;
            if (car.NumberOfPreviousOwners >= 2)
            {
                adjustedAmount = adjustedAmount - (Convert.ToDecimal(adjustedAmount * DISCOUNT));

                if (Component != null)
                {
                    return Component.Calculate(car, adjustedAmount);
                }
            }
            else if (car.NumberOfPreviousOwners == 0)
            {
                if (Component != null)
                {
                    adjustedAmount = Component.Calculate(car, value);
                }
                adjustedAmount = Convert.ToDecimal(adjustedAmount * BONUS);
            }
            else
            {
                if (Component != null)
                {
                    return Component.Calculate(car, adjustedAmount);
                }
            }

            return adjustedAmount;
        }

    }

    public class AdjustCollisionDecorator : Decorator
    {
        private const int COLLISION_MAX = 5;
        public override decimal Calculate(Car car, decimal value)
        {
            var discount1 = Convert.ToDecimal(Math.Min(car.NumberOfCollisions, COLLISION_MAX) * 2 / 100M);
            var adjustedAmount = value - (value * discount1);
            if (Component != null)
            {
                return Component.Calculate(car, adjustedAmount);
            }
            return adjustedAmount;
        }


    }

    [TestFixture]
    public class UnitTests
    {
        [Test]
        public void CalculateCarValue()
        {
            AssertCarValue(25313.40m, 35000m, 3 * 12, 50000, 1, 1);
            AssertCarValue(19688.20m, 35000m, 3 * 12, 150000, 1, 1);
            AssertCarValue(19688.20m, 35000m, 3 * 12, 250000, 1, 1);
            AssertCarValue(20090.00m, 35000m, 3 * 12, 250000, 1, 0);
            AssertCarValue(21657.02m, 35000m, 3 * 12, 250000, 0, 1);
        }

        private static void AssertCarValue(decimal expectValue, decimal purchaseValue,
                                           int ageInMonths, int numberOfMiles, int numberOfPreviousOwners, int
                                               numberOfCollisions)
        {
            Car car = new Car
            {
                AgeInMonths = ageInMonths,
                NumberOfCollisions = numberOfCollisions,
                NumberOfMiles = numberOfMiles,
                NumberOfPreviousOwners = numberOfPreviousOwners,
                PurchaseValue = purchaseValue
            };
            PriceDeterminator priceDeterminator = new PriceDeterminator();
            var carPrice = priceDeterminator.DetermineCarPrice(car);
            Assert.AreEqual(expectValue, carPrice);
        }
    }

}

#region Instructions
/*
 * You are tasked with writing an algorithm that determines the value of a used car, 
 * given several factors.
 * 
 *    AGE:    Given the number of months of how old the car is, reduce its value one-half 
 *            (0.5) percent.
 *            After 10 years, it's value cannot be reduced further by age. This is not 
 *            cumulative.
 *            
 *    MILES:    For every 1,000 miles on the car, reduce its value by one-fifth of a
 *              percent (0.2). Do not consider remaining miles. After 150,000 miles, it's 
 *              value cannot be reduced further by miles.
 *            
 *    PREVIOUS OWNER:    If the car has had more than 2 previous owners, reduce its value 
 *                       by twenty-five (25) percent. If the car has had no previous  
 *                       owners, add ten (10) percent of the FINAL car value at the end.
 *                    
 *    COLLISION:        For every reported collision the car has been in, remove two (2) 
 *                      percent of it's value up to five (5) collisions.
 *                    
 * 
 *    Each factor should be off of the result of the previous value in the order of
 *        1. AGE
 *        2. MILES
 *        3. PREVIOUS OWNER
 *        4. COLLISION
 *        
 *    E.g., Start with the current value of the car, then adjust for age, take that  
 *    result then adjust for miles, then collision, and finally previous owner. 
 *    Note that if previous owner, had a positive effect, then it should be applied 
 *    AFTER step 4. If a negative effect, then BEFORE step 4.
 */
#endregion

